from pathlib import Path
import json,urllib.request,hashlib
root=Path.cwd();p=root/'FireSafetyTraining/Assets/ThirdParty/CampusExterior/floor_tiles_04';p.mkdir(parents=True,exist_ok=True)
def read(u):return urllib.request.urlopen(urllib.request.Request(u,headers={'User-Agent':'AcademicFireSafetyTraining/1.0'}),timeout=90).read()
m=json.loads(read('https://api.polyhaven.com/files/floor_tiles_04'));out=[]
for kind,fmt in [('Diffuse','jpg'),('nor_gl','png'),('Rough','jpg')]:
 f=m[kind]['2k'][fmt];data=read(f['url']);assert hashlib.md5(data).hexdigest()==f['md5'];dest=p/f['url'].rsplit('/',1)[-1];dest.write_bytes(data);out.append(dict(url=f['url'],path=str(dest.relative_to(root)),license='CC0',sha256=hashlib.sha256(data).hexdigest()))
(root/'SourceAssets/CampusExterior/TilesManifest.json').write_text(json.dumps(out,indent=2));print('Verified tiles:',len(out))
