from pathlib import Path
import urllib.request, json, hashlib, zipfile, concurrent.futures
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'SourceAssets/SchoolExpansion'
TARGET=ROOT/'FireSafetyTraining/Assets/ThirdParty/SchoolExpansion'
SOURCE.mkdir(parents=True,exist_ok=True)
TARGET.mkdir(parents=True,exist_ok=True)
def read(url):
    req=urllib.request.Request(url,headers={'User-Agent':'AcademicFireSafetyTraining/1.0 (asset acquisition; Poly Haven credit retained)'})
    for attempt in range(4):
        try: return urllib.request.urlopen(req,timeout=180).read()
        except Exception:
            if attempt==3: raise
            print('Retry download:',url.rsplit('/',1)[-1],flush=True)
records=[]
def download(url,path,md5=None):
    path.parent.mkdir(parents=True,exist_ok=True)
    data=path.read_bytes() if path.exists() else read(url)
    if md5 and hashlib.md5(data).hexdigest()!=md5: raise RuntimeError('Checksum mismatch '+str(path))
    path.write_bytes(data)
    records.append({'url':url,'path':str(path.relative_to(ROOT)),'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest(),'license':'CC0'})
    return data
def poly(asset):
    info=json.loads(read('https://api.polyhaven.com/files/'+asset))
    (SOURCE/(asset+'-files.json')).write_text(json.dumps(info,indent=2))
    meta=json.loads(read('https://api.polyhaven.com/info/'+asset))
    (SOURCE/(asset+'-info.json')).write_text(json.dumps(meta,indent=2))
    f=info['fbx']['1k']['fbx']
    download(f['url'],TARGET/asset/(asset+'.fbx'),f['md5'])
    for name,tex in f.get('include',{}).items(): download(tex['url'],TARGET/asset/name,tex.get('md5'))
    print(asset,'downloaded',flush=True)
def characters():
    url='https://opengameart.org/sites/default/files/ultimate_animated_character_pack_by_quaternius.zip'
    p=SOURCE/'Quaternius-Animated-Characters-CC0.zip';download(url,p)
    with zipfile.ZipFile(p) as z:
        (SOURCE/'CharacterArchiveListing.txt').write_text('\n'.join(z.namelist()))
    print('Character archive downloaded; selection pending',flush=True)
import sys
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
    futures=[pool.submit(poly,x) for x in (sys.argv[1:] or ['tree_small_02','shrub_01','grass_bermuda_01'])]+([] if sys.argv[1:] else [pool.submit(characters)])
    for f in futures:f.result()
(SOURCE/('DownloadManifest-'+('-'.join(sys.argv[1:]) if sys.argv[1:] else 'initial')+'.json')).write_text(json.dumps(records,indent=2))
(TARGET/'LICENSES.txt').write_text('Poly Haven models: CC0. https://polyhaven.com/license\nQuaternius Animated Characters Pack: CC0; publisher upload at https://opengameart.org/content/animated-characters-pack\nSource assets are third-party works. Project integration and alterations documented separately.\n')
print('Saved',len(records),'verified files',flush=True)
