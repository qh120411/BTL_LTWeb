"""Read-only pagination checks; the one POST uses invalid input and never creates a loan."""
import json
import re
import sys
from urllib.parse import urlencode, urlsplit, parse_qs
from urllib.request import Request
import check_pages
from check_pages import Page, login

BASE = sys.argv[1].rstrip('/') if len(sys.argv) > 1 else 'http://localhost:5124'


def main():
    check_pages.BASE = BASE
    session = login('admin', 'Admin@123')
    transactions = json.load(session.open(BASE + '/api/circulation/transactions'))['data']

    def expected(controller, filters):
        rows = transactions
        if controller == 'Overdue':
            rows = sorted([r for r in rows if r['isQuaHan']],
                          key=lambda r: int(re.search(r'\d+', r['trangThai']).group()), reverse=True)
        elif filters.get('status') == 'Quá hạn':
            rows = [r for r in rows if r['isQuaHan']]
        elif filters.get('status') == 'Đang mượn':
            rows = [r for r in rows if not r['isQuaHan']]
        keyword = filters.get('search', '').strip().lower()
        if keyword:
            fields = ['maBD', 'hoTen', 'tenSach'] + (['maGD'] if controller == 'Circulation' else [])
            rows = [r for r in rows if any(keyword in r[f].lower() for f in fields)]
        return rows

    def read(controller, filters, requested_page):
        response = session.open(BASE + '/Admin/' + controller + '?' + urlencode(dict(filters, page=requested_page)))
        body = response.read().decode('utf-8-sig')
        page = Page(body)
        pager = next(a for t, a in page.tags if 'admin-list-footer' in a.get('class', '').split())
        buttons = [a for t, a in page.tags if 'btn-return-trigger' in a.get('class', '').split()]
        rows = expected(controller, filters)
        total_pages = max(1, (len(rows) + 9) // 10)
        actual_page = min(total_pages, max(1, requested_page))
        assert int(pager['data-total']) == len(rows), 'Total changed with paging'
        assert int(pager['data-pages']) == total_pages and int(pager['data-page']) == actual_page
        assert [b['data-magd'] for b in buttons] == [r['maGD'] for r in rows[(actual_page - 1) * 10:actual_page * 10]], 'Rows skipped, repeated or reordered'
        for button in buttons:
            location = urlsplit(button['data-returnurl'])
            assert location.path == '/Admin/' + controller
            query = parse_qs(location.query)
            assert query['page'] == [str(requested_page)]
            for key, value in filters.items():
                assert query[key] == [value], 'Return action lost its filters'
        for t, attrs in page.tags:
            if t == 'a' and any(c.startswith('admin-list-page-') for c in attrs.get('class', '').split()):
                query = parse_qs(urlsplit(attrs['href']).query)
                assert 1 <= int(query['page'][0]) <= total_pages
                for key, value in filters.items():
                    assert query[key] == [value], 'Pagination lost a filter'
        if controller == 'Overdue':
            footer = re.search(r'<tfoot\b.*?</tfoot>', body, re.S).group()
            values = re.findall(r'\d[\d,.]*', re.sub(r'<[^>]+>', ' ', footer))
            assert int(re.sub(r'\D', '', values[-1])) == len(rows) * 100000, 'Fine total includes only the current page'
        return page, buttons

    reader = transactions[0]['maBD'] if transactions else 'unknown'
    cases = [('Circulation', {}), ('Circulation', {'status': 'Quá hạn'}),
             ('Circulation', {'status': 'Đang mượn'}), ('Circulation', {'search': reader}),
             ('Circulation', {'search': 'not-a-real-loan-xyz'}), ('Overdue', {}),
             ('Overdue', {'search': reader, 'level': 'Tất cả'}),
             ('Overdue', {'search': 'not-a-real-loan-xyz'})]
    for controller, filters in cases:
        rows = expected(controller, filters)
        all_ids = []
        total_pages = max(1, (len(rows) + 9) // 10)
        for number in range(1, total_pages + 1):
            _, buttons = read(controller, filters, number)
            assert len(buttons) <= 10
            all_ids.extend(b['data-magd'] for b in buttons)
        assert all_ids == [r['maGD'] for r in rows]
        read(controller, filters, -1)
        read(controller, filters, 2147483647)
        print(f'PASS {controller}: {len(rows)} rows over {total_pages} pages; filters={filters}')

    target = '/Admin/Circulation?' + urlencode({'status': 'Quá hạn', 'page': 2})
    page, _ = read('Circulation', {'status': 'Quá hạn'}, 2)
    token = next(a['value'] for t, a in page.tags if a.get('name') == '__RequestVerificationToken')
    response = session.open(Request(BASE + '/Admin/Circulation/CreateBorrow', data=urlencode({
        '__RequestVerificationToken': token, 'maBD': '', 'bookId': 0, 'returnUrl': target,
    }).encode()))
    assert response.url == BASE + target, 'Borrow validation lost the page/filter context'
    assert json.load(session.open(BASE + '/api/circulation/transactions'))['total'] == len(transactions), 'Invalid verification request changed loan records'
    print('PASS: page/filter context survives borrow validation; no loans created or returned.')


if __name__ == '__main__':
    main()
