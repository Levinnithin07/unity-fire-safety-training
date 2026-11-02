from pathlib import Path
import urllib.request,json,hashlib,re,zipfile,io
root=Path.cwd(); source=root/'SourceAssets/CampusExterior'; source.mkdir(parents=True,exist_ok=True)
target=root/'FireSafetyTraining/Assets/ThirdParty/CampusExterior'; target.mkdir(parents=True,exist_ok=True)
def read(url):
 return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'AcademicFireSafetyTraining/1.0'}),timeout=90).read()
records=[]
page='https://opengameart.org/content/3d-bus'
html=read(page); (source/'Bus-source.html').write_bytes(html)
links=re.findall(r'href="([^"]+\.zip)"',html.decode())
url=next(u for u in links if '3DBus' in u or '3dbus' in u.lower())
data=read(url);(source/'3DBus_by_ajanhallinta.zip').write_bytes(data)
z=zipfile.ZipFile(io.BytesIO(data)); print('Bus archive:',z.namelist())
bus=target/'Bus';bus.mkdir(exist_ok=True)
for n in z.namelist():
 if n.lower().endswith(('.fbx','.obj','.mtl','.txt','.png','.jpg')):
  (bus/Path(n).name).write_bytes(z.read(n))
records.append(dict(asset='3D Bus',author='ajanhallinta',source=page,url=url,license='CC0',sha256=hashlib.sha256(data).hexdigest()))
for asset in ['plastered_wall_02','asphalt_02']:
 meta=json.loads(read('https://api.polyhaven.com/files/'+asset));(source/(asset+'-files.json')).write_text(json.dumps(meta,indent=2))
 folder=target/asset;folder.mkdir(exist_ok=True)
 for kind,fmt in [('Diffuse','jpg'),('nor_gl','png'),('Rough','jpg')]:
  f=meta[kind]['2k' if asset=='plastered_wall_02' else '1k'][fmt];data=read(f['url']);assert hashlib.md5(data).hexdigest()==f['md5']
  p=folder/f['url'].rsplit('/',1)[-1];p.write_bytes(data);records.append(dict(asset=asset,path=str(p.relative_to(root)),url=f['url'],license='CC0',sha256=hashlib.sha256(data).hexdigest()))
(source/'AssetManifest.json').write_text(json.dumps(records,indent=2));print('Downloaded verified files:',len(records))
