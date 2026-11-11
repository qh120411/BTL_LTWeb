"""Check rejection paths only; never submit a valid loan, return, role change or staff creation."""
import json
from urllib.error import HTTPError
from urllib.parse import urlencode
from urllib.request import Request, build_opener
from http.cookiejar import CookieJar
from urllib.request import HTTPCookieProcessor
from check_pages import BASE, Page, login


def request(opener, path, payload=None, token=None):
    headers = {}
    if isinstance(payload, dict):
        payload = json.dumps(payload).encode()
        headers['Content-Type'] = 'application/json'
    if token:
        headers['RequestVerificationToken'] = token
    try:
        response = opener.open(Request(BASE + path, data=payload, headers=headers))
        return response.status, response.read().decode(), response.url
    except HTTPError as error:
        return error.code, error.read().decode(), error.url


def main():
    anonymous = build_opener(HTTPCookieProcessor(CookieJar()))
    admin = login('admin', 'Admin@123')
    student = login('242630923', 'Dat242630923')
    assert json.loads(student.open(BASE + '/api/auth/profile').read())['isAuthenticated']
    protected = ['/api/readers', '/api/readers/242630923', '/api/circulation/transactions', '/api/dashboard/stats']
    for path in protected:
        assert request(anonymous, path)[0] == 401, path
        assert request(student, path)[0] == 403, path
        assert request(admin, path)[0] == 200, path
    print('PASS sensitive JSON endpoints require a staff session; reader sessions are denied.')
    for action in ['borrow', 'return']:
        path = '/api/circulation/' + action
        assert request(anonymous, path, {})[0] == 401
        assert request(student, path, {})[0] == 403
        assert request(admin, path, {})[0] == 400
    page = Page(admin.open(BASE + '/ApiDoc').read().decode())
    token = next(a['value'] for t, a in page.tags if a.get('name') == '__RequestVerificationToken')
    for path, body in [('/api/circulation/borrow', {'maBD': 'SMOKE_UNKNOWN_READER', 'bookId': 999999}),
                       ('/api/circulation/return', {'maBD': 'SMOKE_UNKNOWN_READER', 'maGD': 'SMOKE_INVALID_TRANSACTION'})]:
        status, body, _ = request(admin, path, body, token)
        assert status == 400 and json.loads(body)['success'] is False
    for action in ['AddStaff', 'UpdateRole']:
        path = '/Admin/User/' + action
        assert request(admin, path, b'')[0] == 400
        status, body, _ = request(admin, path, b'', token)
        assert status == 200 and json.loads(body)['success'] is False
    assert request(admin, '/Admin/Reader/ProcessImport', b'')[0] == 400
    print('PASS POST requests enforce antiforgery; invalid requests do not fall back to a successful mutation.')
    for password in ['123456', 'admin123', 'wrong-password-for-smoke-check']:
        isolated = build_opener(HTTPCookieProcessor(CookieJar()))
        page = Page(isolated.open(BASE + '/Account/Login').read().decode())
        csrf = next(a['value'] for t, a in page.tags if a.get('name') == '__RequestVerificationToken')
        isolated.open(Request(BASE + '/Account/Login', data=urlencode({
            'username': 'admin', 'password': password, '__RequestVerificationToken': csrf
        }).encode())).read()
        assert not json.loads(isolated.open(BASE + '/api/auth/profile').read())['isAuthenticated']
    page = Page(anonymous.open(BASE + '/Account/Login').read().decode())
    csrf = next(a['value'] for t, a in page.tags if a.get('name') == '__RequestVerificationToken')
    anonymous.open(Request(BASE + '/Account/LoginWithMicrosoft', data=urlencode({'__RequestVerificationToken': csrf}).encode())).read()
    assert not json.loads(anonymous.open(BASE + '/api/auth/profile').read())['isAuthenticated']
    print('PASS generic fallback passwords and unconfigured SSO cannot sign in as a user.')
    assert request(anonymous, '/Reader/Document/Details/999999')[0] == 404
    assert request(admin, '/api/readers/SMOKE_UNKNOWN_READER')[0] == 404
    for query, page, size in [('page=-1&pageSize=0', 1, 1), ('page=2147483647&pageSize=2147483647', 2147483647, 100)]:
        status, body, _ = request(anonymous, '/api/catalog/books?' + query)
        data = json.loads(body)
        assert status == 200 and data['page'] == page and data['pageSize'] == size
    print('PASS missing document/reader IDs and extreme pagination values.')


if __name__ == '__main__':
    main()
