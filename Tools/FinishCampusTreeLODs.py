from pathlib import Path
import numpy as np,struct,json
root=Path.cwd()/'SourceAssets/CampusExterior'; report=[]
for lod,keep in [(0,3),(1,7)]:
 parts=[]
 with (root/f'tree-lod{lod}.bin').open('rb') as f:
  count=struct.unpack('<i',f.read(4))[0]
  for s in range(count):
   nv,ni=struct.unpack('<ii',f.read(8));v=np.fromfile(f,'<f4',nv*8).reshape(nv,8);t=np.fromfile(f,'<i4',ni).reshape(-1,3)
   if s==1:
    parent=np.arange(nv)
    def find(a):
     while parent[a]!=a: parent[a]=parent[parent[a]];a=parent[a]
     return a
    for a,b,c in t:
     a=find(a);b=find(b);c=find(c);parent[b]=a;parent[c]=a
    groups=np.array([find(int(a)) for a in t[:,0]])
    ids=np.unique(groups);selected=ids[np.arange(len(ids))%keep==0]
    t=t[np.isin(groups,selected)]
    print('Leaf islands',lod,len(ids),'kept',len(selected),flush=True)
   used,remap=np.unique(t,return_inverse=True);v=v[used];t=remap.reshape(-1,3).astype('<i4');parts.append((v,t));report.append(dict(lod=lod,submesh=s,vertices=len(v),triangles=len(t)))
 with (root/f'tree-campus-lod{lod}.bin').open('wb') as f:
  f.write(struct.pack('<i',len(parts)))
  for v,t in parts:f.write(struct.pack('<ii',len(v),t.size));f.write(v.astype('<f4').tobytes());f.write(t.tobytes())
(root/'TreeFinalBudgets.json').write_text(json.dumps(report,indent=2));print(report)
