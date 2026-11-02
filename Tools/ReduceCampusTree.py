from pathlib import Path
import sys,struct,json
sys.path.insert(0,str(Path(__file__).parent/'CampusMeshTools'))
import numpy as np
import fast_simplification as fs
root=Path.cwd(); source=root/'SourceAssets/CampusExterior'
with (source/'tree-source.bin').open('rb') as f:
 n,s=struct.unpack('<ii',f.read(8));data=np.fromfile(f,dtype='<f4',count=n*8).reshape(n,8);subs=[]
 for i in range(s):
  count=struct.unpack('<i',f.read(4))[0];subs.append(np.fromfile(f,dtype='<i4',count=count).reshape(-1,3))
report=[]
for lod,budgets in [(0,[12000,90000,8000]),(1,[4000,28000,2500])]:
 out=[]
 for i,faces in enumerate(subs):
  used,remap=np.unique(faces,return_inverse=True);points=data[used,:3].astype('float64');tri=remap.reshape(-1,3).astype('int32')
  p,t,collapses=fs.simplify(points,tri,target_count=budgets[i],return_collapses=True)
  p,t,mapping=fs.replay_simplification(points,tri,collapses)
  attrs=np.zeros((len(p),5));counts=np.zeros(len(p));np.add.at(attrs,mapping,data[used,3:]);np.add.at(counts,mapping,1);attrs/=np.maximum(counts,1)[:,None]
  out.append((np.column_stack([p,attrs]).astype('<f4'),t.astype('<i4')))
  report.append(dict(lod=lod,submesh=i,original=len(faces),reduced=len(t)));print(report[-1],flush=True)
 with (source/f'tree-lod{lod}.bin').open('wb') as f:
  f.write(struct.pack('<i',len(out)))
  for v,t in out:
   f.write(struct.pack('<ii',len(v),t.size));f.write(v.tobytes());f.write(t.tobytes())
(source/'TreeReduction.json').write_text(json.dumps(report,indent=2))
