"""Verify independent pagination of borrowing and non-borrowing reader tables."""
import json
import sys
from html.parser import HTMLParser
from urllib.parse import urlencode, urlsplit, parse_qs, unquote
import check_pages

BASE = sys.argv[1].rstrip('/') if len(sys.argv) > 1 else 'http://localhost:5124'
GROUPS = ['borrowingReaders', 'nonBorrowingReaders']
PARAMS = ['borrowingPage', 'nonBorrowingPage']


class ReaderPage(HTMLParser):
    def __init__(self, body):
        super().__init__()
        self.depth = self.region_depth = 0
        self.region = None
        self.tables = {key: {'readers': [], 'links': [], 'pager': None} for key in GROUPS}
        self.feed(body)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        classes = attrs.get('class', '').split()
        if tag == 'div':
            self.depth += 1
            if attrs.get('id') in GROUPS:
                self.region = attrs['id']
                self.region_depth = self.depth
        if not self.region:
            return
        table = self.tables[self.region]
        if 'admin-list-footer' in classes:
            table['pager'] = attrs
        if tag == 'a':
            href = attrs.get('href', '')
            if 'btn' in classes and '/Reader/Details/' in href:
                table['readers'].append(unquote(urlsplit(href).path.rsplit('/', 1)[1]))
            elif any(c.startswith('admin-list-page-') for c in classes):
                table['links'].append(href)

    def handle_endtag(self, tag):
        if tag == 'div':
            if self.depth == self.region_depth:
                self.region = None
            self.depth -= 1


def main():
    check_pages.BASE = BASE
    session = check_pages.login('admin', 'Admin@123')
    for filters in [{}, {'group': 'Sinh viên'}, {'group': 'Giảng viên'}, {'group': 'Ngoài trường'},
                    {'search': 'GV'}, {'search': 'dat'}, {'search': 'not-a-real-reader-xyz'}]:
        query = {'keyword' if k == 'search' else k: v for k, v in filters.items()}
        readers = json.load(session.open(BASE + '/api/readers?' + urlencode(query)))['data']
        expected = [[r['maBD'] for r in readers if r['soSachDangMuon'] > 0],
                    [r['maBD'] for r in readers if r['soSachDangMuon'] == 0]]
        pages = [max(1, (len(rows) + 9) // 10) for rows in expected]

        def read(requested):
            params = dict(filters, **dict(zip(PARAMS, requested)))
            body = session.open(BASE + '/Admin/Reader?' + urlencode(params)).read().decode('utf-8-sig')
            view = ReaderPage(body)
            actual = [min(pages[i], max(1, requested[i])) for i in range(2)]
            for i, key in enumerate(GROUPS):
                table = view.tables[key]
                pager = table['pager']
                assert pager is not None, 'Reader table missing pagination'
                assert int(pager['data-total']) == len(expected[i]), 'Reader total changes with the page'
                assert int(pager['data-page']) == actual[i] and int(pager['data-pages']) == pages[i]
                assert table['readers'] == expected[i][(actual[i] - 1) * 10:actual[i] * 10], 'Readers skipped, repeated or reordered'
                for href in table['links']:
                    link = urlsplit(href)
                    query = parse_qs(link.query)
                    assert link.fragment == key
                    assert 1 <= int(query[PARAMS[i]][0]) <= pages[i]
                    assert query[PARAMS[1 - i]] == [str(actual[1 - i])], 'Changing one table reset the other table'
                    for field, value in filters.items():
                        assert query[field] == [value], 'Reader pagination lost a filter'
            return view

        for i in range(2):
            ids = []
            for p in range(1, pages[i] + 1):
                requested = [min(2, count) for count in pages]
                requested[i] = p
                view = read(requested)
                ids.extend(view.tables[GROUPS[i]]['readers'])
            assert ids == expected[i], 'Not all readers can be reached'
        read([-1, 2147483647])
        read([2147483647, -1])
        print(f'PASS reader pagination: {filters}; borrowing={len(expected[0])}/{pages[0]} pages, non-borrowing={len(expected[1])}/{pages[1]} pages')
    print('PASS: independent reader pages, preserved search/group, complete reader lists, empty results and bounded page numbers.')


if __name__ == '__main__':
    main()
