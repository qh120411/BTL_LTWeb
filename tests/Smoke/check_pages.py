"""Read-only HTTP checks for public, reader and admin pages of the local demo."""
import json
import re
import subprocess
import sys
from collections import Counter
from html.parser import HTMLParser
from http.cookiejar import CookieJar
from urllib.error import HTTPError
from urllib.parse import urlencode, urljoin, urlsplit, unquote
from urllib.request import build_opener, HTTPCookieProcessor, Request

BASE = sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5124'


class Page(HTMLParser):
    def __init__(self, body):
        super().__init__()
        self.tags = []
        self.scripts = []
        self.in_script = False
        self.feed(body)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        self.tags.append((tag, attrs))
        if tag == 'script':
            self.in_script = not attrs.get('src') and attrs.get('type', '') not in ['application/ld+json', 'application/json']

    def handle_endtag(self, tag):
        if tag == 'script':
            self.in_script = False

    def handle_data(self, data):
        if self.in_script:
            self.scripts.append(data)


def login(username, password):
    opener = build_opener(HTTPCookieProcessor(CookieJar()))
    body = opener.open(BASE + '/Account/Login').read().decode()
    token = next(a['value'] for t, a in Page(body).tags if a.get('name') == '__RequestVerificationToken')
    opener.open(Request(BASE + '/Account/Login', data=urlencode({
        'username': username, 'password': password, '__RequestVerificationToken': token,
        'returnUrl': '/Reader/Home',
    }).encode())).read()
    return opener


def main():
    sessions = {
        'public': build_opener(),
        'student': login('242630923', 'Dat242630923'),
        'admin': login('admin', 'Admin@123'),
    }
    public = ['/', '/Reader/Home', '/Reader/Search', '/Reader/Search?keyword=khong-co-sach-xyz',
              '/Reader/Guide', '/Reader/News', '/Account/Login', '/Account/AccessDenied', '/ApiDoc']
    private = ['/Reader/Profile', '/Reader/MyLibrary', '/Reader/MyLibrary/BorrowHistory', '/Reader/MyLibrary/Notifications']
    admin = ['/Admin', '/Admin/Dashboard', '/Admin/Catalog', '/Admin/Catalog?search=khong-co-sach-xyz',
             '/Admin/Reader', '/Admin/Reader?search=khong-co-ban-doc-xyz', '/Admin/Reader/Details/242630923',
             '/Admin/Reader/ImportQldt', '/Admin/Circulation', '/Admin/Overdue', '/Admin/Incident',
             '/Admin/Report', '/Admin/User']
    errors = []
    pages = {}
    assets = set()
    pending = [(role, url) for role in sessions for url in public + (admin if role == 'admin' else private if role != 'public' else [])]
    seen = set()
    while pending:
        role, url = pending.pop(0)
        if (role, url) in seen:
            continue
        seen.add((role, url))
        try:
            response = sessions[role].open(BASE + url)
            body = response.read().decode('utf-8-sig')
            if '/Account/Login' in response.url and '/Account/Login' not in url:
                errors.append(f'{role} {url}: unexpected login redirect')
            page = Page(body)
            pages[(role, url)] = page
            counts = Counter(a['id'] for t, a in page.tags if a.get('id'))
            for id_, count in counts.items():
                if count > 1:
                    errors.append(f'{role} {url}: duplicate id {id_} ({count})')
            ids = set(counts)
            for tag, attrs in page.tags:
                target = attrs.get('data-bs-target', '')
                if target.startswith('#') and target[1:] not in ids:
                    errors.append(f'{role} {url}: missing modal/collapse target {target}')
                if tag == 'form' and attrs.get('method', 'get').lower() == 'post':
                    # The token is checked page-wide because nested form nodes aren't needed for this audit.
                    if not any(a.get('name') == '__RequestVerificationToken' for t, a in page.tags):
                        errors.append(f'{role} {url}: POST form has no antiforgery token')
                resource = attrs.get('src') if tag in ['img', 'script'] else attrs.get('href') if tag == 'link' else None
                if resource:
                    parsed = urlsplit(urljoin(BASE + url, resource))
                    if parsed.netloc == urlsplit(BASE).netloc:
                        assets.add(parsed.path)
                href = attrs.get('href', '') if tag == 'a' else ''
                parsed = urlsplit(urljoin(BASE + url, href))
                if href and parsed.netloc == urlsplit(BASE).netloc and re.fullmatch(r'/Reader/(Document|News)/Details/\d+', parsed.path):
                    siblings = sum(1 for r, u in seen.union(set(pending)) if r == role and u.startswith('/'.join(parsed.path.split('/')[:-1])))
                    if siblings < 3:
                        pending.append((role, parsed.path))
        except (HTTPError, UnicodeDecodeError) as error:
            errors.append(f'{role} {url}: {error}')
    for asset in sorted(assets):
        try:
            sessions['public'].open(BASE + asset).read()
        except HTTPError as error:
            errors.append(f'asset {unquote(asset)}: HTTP {error.code}')
    scripts = [{'page': role + ' ' + url, 'script': script} for (role, url), page in pages.items() for script in page.scripts]
    scripts.extend({'page': role + ' ' + url + ' ' + key, 'script': 'function handler(event) {' + value + '}'}
                   for (role, url), page in pages.items() for tag, attrs in page.tags
                   for key, value in attrs.items() if key.startswith('on') and value)
    check = subprocess.run(['node', '-e', "const vm=require('node:vm');let s='';process.stdin.on('data',c=>s+=c);process.stdin.on('end',()=>{for(const x of JSON.parse(s)){try{new vm.Script(x.script)}catch(e){console.log(x.page+': '+e.message)}}})"],
                           input=json.dumps(scripts), text=True, capture_output=True, check=True)
    errors.extend('JS ' + line for line in check.stdout.splitlines())
    # All fragment links between inspected pages must resolve to an existing target.
    fragments = set()
    for (role, url), page in pages.items():
        for tag, attrs in page.tags:
            href = attrs.get('href', '') if tag == 'a' else ''
            parsed = urlsplit(urljoin(BASE + url, href))
            if not href or not parsed.fragment or href == '#' or parsed.netloc != urlsplit(BASE).netloc:
                continue
            target = pages.get((role, parsed.path))
            if target and not any(a.get('id') == parsed.fragment for t, a in target.tags):
                fragments.add(f'link {parsed.path}#{parsed.fragment}: missing section')
    errors.extend(sorted(fragments))
    print(f'Checked {len(pages)} rendered pages across {len(sessions)} sessions, {len(assets)} local assets, {len(scripts)} inline scripts.')
    for error in sorted(set(errors)):
        print('FAIL', error)
    if not errors:
        print('PASS page responses, IDs, modal targets, tokens, local resources and JavaScript syntax.')
    return bool(errors)


if __name__ == '__main__':
    sys.exit(main())
