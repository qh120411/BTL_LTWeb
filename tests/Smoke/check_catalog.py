"""Read-only checks for the admin catalog cards, filters, pagination and CRUD bindings."""
import json
import sys
from pathlib import Path
from urllib.parse import urlencode, urlsplit, parse_qs
import check_pages
from check_pages import Page, login

BASE = sys.argv[1].rstrip('/') if len(sys.argv) > 1 else 'http://localhost:5124'
ROOT = Path(__file__).resolve().parents[2]


def main():
    check_pages.BASE = BASE
    session = login('admin', 'Admin@123')
    source = json.loads((ROOT / 'App_Data/catalog_books.json').read_text(encoding='utf-8-sig'))
    cases = [{}, {'major': 'cntt'}, {'major': 'kinhte'}, {'category': 'books'}, {'category': 'thesis'},
             {'category': 'journal'}, {'language': 'vie'}, {'language': 'eng'}, {'status': 'Có sẵn'},
             {'status': 'Hết sách'}, {'status': 'Bản điện tử'}, {'search': 'Python'},
             {'major': 'cntt', 'category': 'books', 'language': 'vie', 'status': 'Có sẵn'},
             {'search': 'not-a-real-title-xyz'}]

    def expected(params):
        books = source
        for key, field in [('major', 'Major'), ('category', 'Category'), ('language', 'Language')]:
            if params.get(key):
                books = [b for b in books if b[field].lower() == params[key].lower()]
        if params.get('status') == 'Có sẵn':
            books = [b for b in books if not b['IsOnline'] and b['AvailableCopies'] > 0]
        elif params.get('status') == 'Hết sách':
            books = [b for b in books if not b['IsOnline'] and b['AvailableCopies'] <= 0]
        elif params.get('status') == 'Bản điện tử':
            books = [b for b in books if b['IsOnline']]
        if params.get('search'):
            query = params['search'].strip().lower()
            books = [b for b in books if any(query in (b.get(k) or '').lower() for k in ['Title', 'Authors', 'Publisher', 'ISBN'])]
        return books

    def read(params):
        response = session.open(BASE + '/Admin/Catalog?' + urlencode(params))
        assert '/Account/Login' not in response.url, 'Admin login failed'
        page = Page(response.read().decode('utf-8-sig'))
        results = next(a for t, a in page.tags if a.get('id') == 'catalogResults')
        cards = [int(a['data-book-id']) for t, a in page.tags if t == 'article' and 'data-book-id' in a]
        for class_ in ['btn-view-copies', 'btn-edit-book', 'btn-delete-book']:
            ids = [int(a['data-id']) for t, a in page.tags if t == 'button' and class_ in a.get('class', '').split()]
            assert ids == cards, f'{class_} does not match the current cards'
        forms = [a for t, a in page.tags if t == 'form' and a.get('method', '').lower() == 'post']
        assert {urlsplit(a['action']).path for a in forms} >= {'/Admin/Catalog/Create', '/Admin/Catalog/Update', '/Admin/Catalog/Delete'}, 'CRUD forms were removed'
        assert any(a.get('name') == '__RequestVerificationToken' for t, a in page.tags), 'Missing CSRF token'
        for t, a in page.tags:
            if t == 'a' and 'catalog-page' in a.get('class', ''):
                query = parse_qs(urlsplit(a['href']).query)
                for field, value in params.items():
                    if field != 'page':
                        assert query.get(field) == [value], f'Pagination lost {field}'
        return results, cards

    for params in cases:
        books = expected(params)
        results, _ = read(params)
        assert int(results['data-total']) == len(books), f'Incorrect filtered count: {params}'
        page_count = max(1, (len(books) + 11) // 12)
        assert int(results['data-pages']) == page_count
        all_ids = []
        for number in range(1, page_count + 1):
            current, cards = read(dict(params, page=number))
            assert int(current['data-page']) == number
            assert cards == [b['Id'] for b in books[(number - 1) * 12:number * 12]], 'Cards skipped/repeated or reordered'
            all_ids.extend(cards)
        assert all_ids == [b['Id'] for b in books]
        print(f'PASS catalog: {params}; {len(books)} titles across {page_count} pages')
    for value, target in [(-1, 1), (2147483647, max(1, (len(source) + 11) // 12))]:
        result, _ = read({'page': value})
        assert int(result['data-page']) == target, 'Out-of-range page was not clamped'
    for book in [source[0], source[-1]]:
        details = json.load(session.open(BASE + '/Admin/Catalog/GetBook/' + str(book['Id'])))
        copies = json.load(session.open(BASE + '/Admin/Catalog/GetCopies/' + str(book['Id'])))
        assert details['id'] == book['Id'] and details['title'] == book['Title'], 'Edit modal loads the wrong book'
        assert copies['bookId'] == book['Id'], 'Copies modal loads the wrong book'
    print('PASS: all cards retain CRUD controls, modal data, CSRF and bounded pagination; no catalog records changed.')


if __name__ == '__main__':
    main()
