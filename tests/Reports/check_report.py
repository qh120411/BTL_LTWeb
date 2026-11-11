"""Verify report pagination, filters, aggregate totals and the complete Excel export."""
import sys
from html.parser import HTMLParser
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import urlencode, urlsplit, parse_qs
from urllib.request import Request, build_opener

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'Smoke'))
import check_pages


def number(text):
    return int(''.join(c for c in text if c.isdigit()))


class ReportHTML(HTMLParser):
    def __init__(self, body, excel=False):
        super().__init__()
        self.excel = excel
        self.in_inventory = excel
        self.summary = []
        self.rows = []
        self.row = self.cell = self.summary_value = None
        self.inputs = {}
        self.total_titles = self.page_count = self.page = 0
        self.export_link = None
        self.pagination_links = []
        self.current_pages = []
        self.feed(body)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if attrs.get('aria-current') == 'page':
            self.current_pages.append(number(attrs['aria-label']))
        if tag == 'table' and not self.excel:
            self.in_inventory = attrs.get('id') == 'reportInventory'
            if self.in_inventory:
                self.total_titles = int(attrs['data-total-titles'])
                self.page_count = int(attrs['data-total-pages'])
                self.page = int(attrs['data-page'])
        if tag == 'input' and attrs.get('name'):
            self.inputs[attrs['name']] = attrs.get('value', '')
        if tag == 'a':
            href = attrs.get('href', '')
            if '/Report/ExportExcel' in href:
                self.export_link = href
            if 'page=' in href:
                self.pagination_links.append(href)
        if tag == 'tr' and self.in_inventory:
            self.row = []
        elif tag in ('td', 'th') and self.in_inventory:
            self.cell = []
        elif tag == 'dd':
            self.summary_value = []

    def handle_data(self, data):
        if self.cell is not None:
            self.cell.append(data)
        if self.summary_value is not None:
            self.summary_value.append(data)

    def handle_endtag(self, tag):
        if tag in ('td', 'th') and self.cell is not None:
            if self.row is not None:
                self.row.append(''.join(self.cell).strip())
            self.cell = None
        elif tag == 'tr' and self.row is not None:
            self.rows.append(self.row)
            self.row = None
        elif tag == 'dd' and self.summary_value is not None:
            self.summary.append(number(''.join(self.summary_value)))
            self.summary_value = None
        elif tag == 'table' and not self.excel:
            self.in_inventory = False


def main():
    if len(sys.argv) > 1:
        check_pages.BASE = sys.argv[1].rstrip('/')
    base = check_pages.BASE
    session = check_pages.login('admin', 'Admin@123')

    def get_page(params):
        response = session.open(base + '/Admin/Report?' + urlencode(params))
        assert '/Account/Login' not in response.url, 'Admin login failed'
        body = response.read().decode('utf-8-sig')
        page = ReportHTML(body)
        assert len(page.summary) >= 5 and page.export_link, 'Missing metrics or export'
        # dd elements in the stock legend follow the five summary cards.
        page.summary = page.summary[:5]
        assert 'reportScope' not in body and 'Số liệu minh họa' not in body, 'Stale demo data'
        assert len(page.rows) >= 2, 'Missing inventory table'
        return page

    default = get_page({})
    first_title = default.rows[1][0] if default.total_titles else 'no-book'
    current_start, current_end = default.inputs['startDate'], default.inputs['endDate']
    cases = [
        {}, {'period': 'Tất cả thời gian'},
        {'startDate': current_start, 'endDate': current_end, 'search': first_title[:8]},
        {'startDate': current_start, 'endDate': current_start},
        {'search': 'not-a-real-book-xyz'}, {'page': 2147483647}, {'page': -1},
    ]
    for params in cases:
        page = get_page(params)
        all_details = []
        for p in range(1, page.page_count + 1):
            current = page if p == page.page else get_page(dict(params, page=p))
            assert current.summary == page.summary, 'Global metrics changed with pagination'
            assert current.rows[-1] == page.rows[-1], 'Filtered totals changed with pagination'
            partial_response = session.open(Request(base + '/Admin/Report?' + urlencode(dict(params, page=p)), headers={'X-Report-Partial': 'inventory'}))
            partial_body = partial_response.read().decode('utf-8-sig')
            fragment = ReportHTML(partial_body)
            assert '<html' not in partial_body.lower() and '<script' not in partial_body.lower(), 'Async pagination returned a full page'
            assert fragment.rows == current.rows and fragment.page == current.page, 'Async table differs from the normal page'
            assert 'no-store' in partial_response.headers.get('Cache-Control', ''), 'Private report should not be cached'
            if current.page_count > 1:
                assert fragment.current_pages == [p], 'Pager must identify exactly one current page'
                targets = {int(parse_qs(urlsplit(href).query)['page'][0]) for href in fragment.pagination_links}
                for endpoint in [1, current.page_count]:
                    assert endpoint == p or endpoint in targets, 'Pager lost the first or last page'
            details = current.rows[1:-1] if current.total_titles else []
            assert len(details) <= 10, 'Inventory page exceeds ten rows'
            all_details.extend(details)
            for row in details:
                values = [number(c) for c in row[1:]]
                assert values[0] == sum(values[1:]), 'Inventory row does not reconcile'
        assert len(all_details) == page.total_titles, 'Pagination skipped inventory rows'
        totals = [number(c) for c in page.rows[-1][1:]]
        assert totals == [sum(number(row[i + 1]) for row in all_details) for i in range(len(totals))], 'Filtered footer does not match all pages'
        assert page.summary[3:] == default.summary[3:], 'Search or date filters changed global stock/readers'
        for href in page.pagination_links:
            query = parse_qs(urlsplit(href).query)
            assert query.get('search', [''])[0] == page.inputs.get('search', ''), 'Pagination lost search'
            assert query.get('startDate', [''])[0] == page.inputs.get('startDate', ''), 'Pagination lost date filter'
            assert query.get('endDate', [''])[0] == page.inputs.get('endDate', ''), 'Pagination lost end date'

        export = session.open(base + page.export_link)
        assert 'application/vnd.ms-excel' in export.headers['Content-Type'], 'Wrong export content type'
        excel = ReportHTML(export.read().decode('utf-8-sig'), excel=True)
        metrics = {row[0]: number(row[1]) for row in excel.rows if len(row) == 2 and row[1].isdigit()}
        assert [metrics[k] for k in ['Lượt mượn trong kỳ', 'Lượt trả trong kỳ', 'Đang quá hạn', 'Tổng bản sách hiện tại', 'Tổng bạn đọc']] == page.summary, 'Excel metrics differ from page'
        header = next(i for i, row in enumerate(excel.rows) if row[0] == 'Tên sách')
        assert excel.rows[header + 1:-1] == all_details, 'Excel omitted matching rows beyond current page'
        assert excel.rows[-1] == page.rows[-1], 'Excel totals differ from page'
        print(f'PASS HTTP + Excel: {params}; {page.total_titles} titles, {page.page_count} pages; summary={page.summary}')

    for action in ['', '/ExportExcel']:
        try:
            session.open(base + '/Admin/Report' + action + '?' + urlencode({'startDate': '2026-12-01', 'endDate': '2026-11-01'}))
            raise AssertionError('Invalid range must be rejected')
        except HTTPError as error:
            assert error.code == 400, f'Unexpected validation status: {error.code}'
    print('PASS: invalid dates rejected on report and export.')
    for role, opener in [('public', build_opener()), ('student', check_pages.login('242630923', 'Dat242630923'))]:
        response = opener.open(Request(base + '/Admin/Report?page=2', headers={'X-Report-Partial': 'inventory'}))
        assert '/Admin/Report' not in response.url, f'{role} accessed the private inventory fragment'
    print('PASS: inventory fragments retain admin authorization.')


if __name__ == '__main__':
    main()
