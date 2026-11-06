const {chromium}=require('playwright');
const assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch({headless:true});
 const context=await browser.newContext({viewport:{width:1920,height:1080}}),p=await context.newPage(),errs=[];
 p.on('pageerror',e=>errs.push(e.message));p.setDefaultTimeout(8000);const base=process.env.UTC_TEST_URL||'http://127.0.0.1:5124';
 async function go(url){const r=await p.goto(base+url);assert(r.status()<400,`${url} ${r.status()}`);await p.evaluate(()=>document.fonts.ready);return r}
 async function post(url,form){const token=await p.locator('input[name=__RequestVerificationToken]').first().inputValue();const r=await context.request.post(base+url,{form:{...form,__RequestVerificationToken:token}});assert(r.status()<400,`${url} ${r.status()} ${(await r.text()).slice(0,200)}`);return r}
 await go('/');assert(p.url().includes('/Reader'));assert.equal(await p.locator('.home-book').count(),4);await p.screenshot({path:require('node:path').join(process.env.UTC_TEST_OUTPUT,'home-desktop.png'),fullPage:true});
 await p.locator('#home-query').fill('xu ly anh');await p.getByRole('button',{name:'Tìm học liệu'}).click();assert.equal(await p.locator('.catalog-card').count(),1);
 const missing=await context.request.get(base+'/Reader/Document/Details/999999');assert.equal(missing.status(),404);
 await go('/Admin/Dashboard');assert(p.url().includes('/Account/Login'));
 await post('/Account/Login',{username:'admin',password:'123456'});await go('/Admin/Dashboard');assert(p.url().includes('/Account/Login'));
 await p.locator('#username').fill('242630944');await p.locator('#password').fill('UTC@123');await p.getByRole('button',{name:'Đăng nhập',exact:true}).click();await p.waitForURL('**/Reader/MyLibrary');
 const denied=await context.request.get(base+'/Admin/Dashboard');assert.equal(denied.status(),403);
 const noCsrf=await context.request.post(base+'/Reader/Profile',{form:{Email:'test@utc.edu.vn'}});assert.equal(noCsrf.status(),400);
 await go('/Reader/Profile');await post('/Reader/Profile',{Email:'huy-test@st.utc.edu.vn',SoDienThoai:'0901234567',HoTen:'Không được đổi tên'});await go('/Reader/Profile');assert.equal(await p.locator('[name=Email]').inputValue(),'huy-test@st.utc.edu.vn');assert(await p.getByText('Trần Quang Huy · 242630944').isVisible());
 await post('/Account/Logout',{});await go('/Account/Login');await post('/Account/Login',{username:'admin',password:'Admin@123',returnUrl:'https://example.com'});
 await go('/Admin/Dashboard');assert.equal(await p.locator('.stat strong').first().textContent(),'4');await p.screenshot({path:require('node:path').join(process.env.UTC_TEST_OUTPUT,'admin-desktop.png'),fullPage:true});
 await go('/Admin/Catalog');await post('/Admin/Catalog/Create',{title:'Sách kiểm thử MVC',authors:'Tác giả thử',publisher:'UTC',major:'cntt',year:'2026',pages:'200',isbn:'TEST-MVC-01',shelf:'A8-06',copiesCount:'2'});
 await go('/Reader/Search?keyword=Sách%20kiểm%20thử%20MVC');assert.equal(await p.locator('.catalog-card').count(),1);
 await p.locator('.catalog-card').click();const bookId=p.url().split('/').at(-1);assert.equal(await p.locator('tbody tr').count(),2);
 const barcode=await p.locator('tbody tr').first().locator('td').first().textContent();
 await go('/Admin/Catalog');await post('/Admin/Catalog/Update',{id:bookId,title:'Sách kiểm thử MVC',authors:'Tác giả thử',publisher:'UTC',major:'cntt',year:'2026',pages:'200',isbn:'TEST-MVC-01',shelf:'A8-06'});
 await go('/Admin/Catalog');await post('/Admin/Catalog/Update',{id:bookId,title:'Sách kiểm thử MVC',authors:'Tác giả mới',publisher:'UTC',major:'cntt',year:'2026',pages:'200',isbn:'TEST-MVC-01',shelf:'A8-06'});await go('/Reader/Document/Details/'+bookId);assert(await p.getByText('Tác giả mới',{exact:true}).isVisible());

 await go('/Admin/Circulation');await post('/Admin/Circulation/CreateBorrow',{maBD:'242630944',bookId});await go('/Admin/Circulation');assert.equal(await p.locator('tbody tr').count(),1);const transaction=await p.locator('tbody tr').first().locator('td').first().textContent();
 await post('/Admin/Circulation/CreateBorrow',{maBD:'242630944',bookId});await go('/Admin/Circulation');assert.equal(await p.locator('tbody tr').count(),1);
 await post('/Admin/Circulation/ProcessReturnOrCompensation',{maBD:'242630944',maGD:transaction,copyBarcode:'WRONG',actionType:'NormalReturn'});await go('/Admin/Circulation');assert.equal(await p.locator('tbody tr').count(),1);
 await post('/Admin/Circulation/ProcessReturnOrCompensation',{maBD:'242630944',maGD:transaction,copyBarcode:barcode,actionType:'NormalReturn'});await go('/Admin/Circulation');assert.equal(await p.locator('tbody tr').count(),0);
 await go('/Reader/Document/Details/'+bookId);assert.match(await p.locator('.badge').textContent(),/2 \/ 2/);
 await go('/Admin/Circulation');await post('/Admin/Circulation/CreateBorrow',{maBD:'242630944',bookId});await go('/Admin/Circulation');const t2=await p.locator('tbody tr').first().locator('td').first().textContent();
 await post('/Admin/Circulation/ProcessReturnOrCompensation',{maBD:'242630944',maGD:t2,copyBarcode:barcode,actionType:'Compensation',compensationType:'Money',compensationAmount:'150000',payOverdue:'false'});
 await go('/Admin/Incident');assert.match(await p.locator('tbody').textContent(),/150,000|150\.000|150000/);
 await go('/Reader/Document/Details/'+bookId);assert.match(await p.locator('.badge').textContent(),/1 \/ 2/);
 await go('/Admin/Catalog');await post('/Admin/Catalog/Delete',{id:bookId});await go('/Reader/Document/Details/'+bookId);
 await go('/Admin/Catalog');await post('/Admin/Catalog/Create',{title:'Xóa thử',authors:'Tác giả',publisher:'UTC',major:'cntt',year:'2026',pages:'50',copiesCount:'1'});await go('/Reader/Search?keyword=Xóa%20thử');await p.locator('.catalog-card').click();const deleteId=p.url().split('/').at(-1);await go('/Admin/Catalog');await post('/Admin/Catalog/Delete',{id:deleteId});assert.equal((await context.request.get(base+'/Reader/Document/Details/'+deleteId)).status(),404);

 await go('/Admin/Reader/Details/242630945');await post('/Admin/Reader/UpdateProfile',{MaBD:'242630945',QuyenSuDung:'Bị khóa',HieuLucQuyen:'2028-01-01'});await go('/Admin/Circulation');await post('/Admin/Circulation/CreateBorrow',{maBD:'242630945',bookId:'1'});await go('/Admin/Circulation');assert.equal(await p.locator('tbody tr').count(),0);
 await go('/Admin/Reader/ImportQldt');await post('/Admin/Reader/ImportTsv',{rows:'242639991\tSinh viên mới\tnew@st.utc.edu.vn\n242630944\tTrùng mã\tother@st.utc.edu.vn'});await go('/Admin/Reader');assert.equal(await p.locator('tbody tr').count(),2);
 await go('/Admin/Reader/ImportQldt');await post('/Admin/Reader/ImportTsv',{rows:'242639991\tSinh viên mới\tnew@st.utc.edu.vn'});await go('/Admin/Reader');assert.equal(await p.locator('tbody tr').count(),3);
 const csv=await context.request.get(base+'/Admin/Report/ExportExcel');assert.equal(csv.status(),200);assert.match(csv.headers()['content-type'],/text\/csv/);assert((await csv.text()).includes('Sách kiểm thử MVC'));
 const routes=['/Reader/Home','/Reader/Search','/Reader/Document/Details/1','/Reader/Guide','/Reader/News','/Account/Login','/Admin/Dashboard','/Admin/Catalog','/Admin/Reader','/Admin/Reader/Details/242630944','/Admin/Reader/ImportQldt','/Admin/Circulation','/Admin/Overdue','/Admin/Incident','/Admin/Report','/Admin/User'];
 const overflows=[],bad=[];p.on('response',r=>{if(r.url().startsWith(base)&&r.status()>=400)bad.push([r.status(),r.url()])});
 for(const width of [1920,1366,768,390,375]){await p.setViewportSize({width,height:900});for(const route of routes){await go(route);const dims=await p.evaluate(()=>({width:innerWidth,scroll:document.documentElement.scrollWidth}));if(dims.scroll>width+1)overflows.push({route,...dims})}}
 await p.setViewportSize({width:390,height:844});await go('/Reader/Home');await p.screenshot({path:require('node:path').join(process.env.UTC_TEST_OUTPUT,'home-mobile.png'),fullPage:true});await p.locator('[data-menu]').click();assert.equal(await p.locator('[data-menu]').getAttribute('aria-expanded'),'true');
 assert.deepEqual(overflows,[]);assert.deepEqual(bad,[]);assert.deepEqual(errs,[]);
 console.log(JSON.stringify({passed:true,viewportChecks:80,flows:['public OPAC','accent-insensitive search','404 detail','login rejects fallback','reader cannot access admin','CSRF rejection','profile persistence/identity protected','admin login','SQL catalog add','SQL borrow','duplicate loan blocked','wrong barcode blocked','return restores stock','cash compensation records payment without restoring lost stock','locked card blocked','atomic TSV import','CSV report'],overflows,bad,errs}));
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});
