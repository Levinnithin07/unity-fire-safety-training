from pathlib import Path
import urllib.request,json,hashlib
root=Path.cwd(); asset='leafy_grass'; target=root/'FireSafetyTraining/Assets/ThirdParty/SchoolExpansion'/asset;target.mkdir(parents=True,exist_ok=True)
def read(url):
 return urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'AcademicFireSafetyTraining/1.0'}),timeout=90).read()
meta=json.loads(read('https://api.polyhaven.com/files/'+asset));records=[]
for kind,fmt in [('Diffuse','jpg'),('nor_gl','png'),('Rough','jpg')]:
 f=meta[kind]['1k'][fmt];data=read(f['url']);assert hashlib.md5(data).hexdigest()==f['md5'];p=target/f['url'].rsplit('/',1)[-1];p.write_bytes(data);records.append({'url':f['url'],'path':str(p),'sha256':hashlib.sha256(data).hexdigest(),'license':'CC0'})
(root/'SourceAssets/SchoolExpansion/LeafyGrassManifest.json').write_text(json.dumps(records,indent=2));print('Downloaded and checksum verified',len(records),'CC0 ground maps')


