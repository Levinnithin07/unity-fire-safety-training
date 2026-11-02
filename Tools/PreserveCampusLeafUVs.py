from pathlib import Path
import numpy as np,struct,json
root=Path.cwd()/'SourceAssets/CampusExterior'
with (root/'tree-source.bin').open('rb') as f:
 n,s=struct.unpack('<ii',f.read(8));data=np.fromfile(f,'<f4',n*8).reshape(n,8);subs=[]
 for i in range(s):
  count=struct.unpack('<i',f.read(4))[0];subs.append(np.fromfile(f,'<i4',count).reshape(-1,3))
t=subs[1];parent=np.arange(n)
def find(a):
 while parent[a]!=a: parent[a]=parent[parent[a]];a=parent[a]
 return a
for a,b,c in t:
 a=find(a);b=find(b);c=find(c);parent[b]=a;parent[c]=a
groups=np.array([find(int(a)) for a in t[:,0]]);ids=np.unique(groups)
report=[]
for lod,step,scale in [(0,8,2.0),(1,20,2.6)]:
 parts=[]
 with (root/f'tree-campus-lod{lod}.bin').open('rb') as f:
  count=struct.unpack('<i',f.read(4))[0]
  for s in range(count):
   nv,ni=struct.unpack('<ii',f.read(8));v=np.fromfile(f,'<f4',nv*8).reshape(nv,8);faces=np.fromfile(f,'<i4',ni).reshape(-1,3);parts.append((v,faces))
 selected=ids[::step];mask=np.isin(groups,selected);faces=t[mask];used,remap=np.unique(faces,return_inverse=True);v=data[used].copy();vg=np.array([find(int(a)) for a in used]);unique,inv=np.unique(vg,return_inverse=True);centres=np.zeros((len(unique),3));counts=np.bincount(inv);np.add.at(centres,inv,v[:,:3]);centres/=counts[:,None];v[:,:3]=centres[inv]+(v[:,:3]-centres[inv])*scale
 parts[1]=(v,remap.reshape(-1,3).astype('<i4'))
 with (root/f'tree-campus-lod{lod}.bin').open('wb') as f:
  f.write(struct.pack('<i',len(parts)))
  for v,t2 in parts:f.write(struct.pack('<ii',len(v),t2.size));f.write(v.astype('<f4').tobytes());f.write(t2.astype('<i4').tobytes())
 report.append(dict(lod=lod,totalTriangles=sum(len(t2) for v,t2 in parts),leafIslands=len(selected),method='Original leaf UVs and geometry retained; sampled islands enlarged around their centers'))
(root/'TreeFinalBudgets.json').write_text(json.dumps(report,indent=2));print(report)
