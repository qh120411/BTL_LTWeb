"""Build và thử HTTP/browser với database tạm; không thay đổi database của dự án."""
from pathlib import Path
import os, subprocess, tempfile, time, urllib.request, socket
root=Path(__file__).resolve().parent.parent
subprocess.run(['dotnet','build',str(root/'BTL_LTWEB.csproj'),'--nologo'],check=True)
with tempfile.TemporaryDirectory(prefix='utc-mvc-test-') as directory:
    with socket.socket() as sock:
        sock.bind(('127.0.0.1',0)); port=sock.getsockname()[1]
    url=f'http://127.0.0.1:{port}'
    env=dict(os.environ,ASPNETCORE_ENVIRONMENT='Development',UTC_TEST_URL=url,UTC_TEST_OUTPUT=directory)
    env['Database__Provider']='Sqlite'; env['Database__SeedDemo']='true'
    env['ConnectionStrings__DemoConnection']='Data Source='+str(Path(directory)/'test.db')
    with open(Path(directory)/'server.log','w+') as log:
        p=subprocess.Popen(['dotnet',str(root/'bin/Debug/net8.0/BTL_LTWEB.dll'),'--urls',url],cwd=root,env=env,stdout=log,stderr=log)
        try:
            ready=False
            for _ in range(100):
                if p.poll() is not None: break
                try:
                    with urllib.request.urlopen(url+'/Reader/Home',timeout=1) as response:
                        ready=response.status==200
                    if ready: break
                except Exception: time.sleep(.25)
            if not ready:
                log.seek(0); print(log.read()); raise RuntimeError('Website không khởi động được')
            result=subprocess.run(['node',str(root/'Tests/verify.cjs')],env=env)
            if result.returncode:
                log.seek(0); print(log.read()[-10000:])
            raise SystemExit(result.returncode)
        finally:
            p.terminate(); p.wait(timeout=15)
