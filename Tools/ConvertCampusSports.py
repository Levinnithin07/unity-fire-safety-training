"""Convert the two original CC0 GLBs to baked Unity-coordinate mesh buffers.
No runtime importer or third-party package is added to Unity."""
import json, struct, pathlib, hashlib
import numpy as np

root=pathlib.Path(__file__).resolve().parents[1]
out=root/"FireSafetyTraining/Assets/ThirdParty/CampusSports"
out.mkdir(parents=True,exist_ok=True)
records=[]
for name,slug in [
    ("basketball-hoop","wall-basketball-hoop-sports-equipment-7cfb56b9"),
    ("volleyball-net","volleyball-net-sports-equipment-3ddaf116")]:
    src=root/"SourceAssets/CampusSports"/(name+".glb")
    data=src.read_bytes()
    length,kind=struct.unpack_from("<II",data,12)
    doc=json.loads(data[20:20+length])
    blob=data[28+length:]
    dest=out/name;dest.mkdir(exist_ok=True)
    (dest/(name+".glb")).write_bytes(data)
    def accessor(i):
        a=doc["accessors"][i];v=doc["bufferViews"][a["bufferView"]]
        dtypes={5126:"<f4",5125:"<u4",5123:"<u2",5121:"u1"}
        dtype=np.dtype(dtypes[a["componentType"]])
        width={"SCALAR":1,"VEC2":2,"VEC3":3,"VEC4":4}[a["type"]]
        return np.ndarray((a["count"],width),dtype=dtype,buffer=blob,
             offset=v.get("byteOffset",0)+a.get("byteOffset",0),
             strides=(v.get("byteStride",dtype.itemsize*width),dtype.itemsize)).copy()
    groups={i:{"v":[],"n":[],"uv":[],"tri":[]} for i in range(len(doc["materials"]))}
    def walk(index,parent):
        node=doc["nodes"][index]
        if "matrix" in node: mat=np.array(node["matrix"]).reshape(4,4).T
        else:
            x,y,z,w=node.get("rotation",[0,0,0,1])
            rot=np.array([[1-2*y*y-2*z*z,2*x*y-2*z*w,2*x*z+2*y*w],
                [2*x*y+2*z*w,1-2*x*x-2*z*z,2*y*z-2*x*w],
                [2*x*z-2*y*w,2*y*z+2*x*w,1-2*x*x-2*y*y]])
            mat=np.eye(4);mat[:3,:3]=rot@np.diag(node.get("scale",[1,1,1]))
            mat[:3,3]=node.get("translation",[0,0,0])
        world=parent@mat
        if "mesh" in node:
            for p in doc["meshes"][node["mesh"]]["primitives"]:
                g=groups[p.get("material",0)];offset=sum(len(v) for v in g["v"])
                v=accessor(p["attributes"]["POSITION"]);n=accessor(p["attributes"]["NORMAL"])
                v=v@world[:3,:3].T+world[:3,3];n=n@np.linalg.inv(world[:3,:3])
                v[:,2]*=-1;n[:,2]*=-1;n/=np.linalg.norm(n,axis=1)[:,None]
                uv=accessor(p["attributes"]["TEXCOORD_0"]);uv[:,1]=1-uv[:,1]
                tri=accessor(p["indices"]).reshape(-1,3)[:,[0,2,1]]+offset
                g["v"].append(v);g["n"].append(n);g["uv"].append(uv);g["tri"].append(tri)
        for child in node.get("children",[]):walk(child,world)
    for index in doc["scenes"][doc.get("scene",0)]["nodes"]:walk(index,np.eye(4))
    materials=[]
    for i,m in enumerate(doc["materials"]):
        pbr=m.get("pbrMetallicRoughness",{});texture=None
        if "baseColorTexture" in pbr:
            image=doc["images"][doc["textures"][pbr["baseColorTexture"]["index"]]["source"]]
            view=doc["bufferViews"][image["bufferView"]]
            texture=f"material-{i}.png" if image["mimeType"]=="image/png" else f"material-{i}.jpg"
            (dest/texture).write_bytes(blob[view.get("byteOffset",0):view.get("byteOffset",0)+view["byteLength"]])
        materials.append({"name":m["name"],"texture":texture,"color":pbr.get("baseColorFactor",[1,1,1,1]),
            "metallic":pbr.get("metallicFactor",1),"smoothness":1-pbr.get("roughnessFactor",1),
            "glass":"KHR_materials_transmission" in m.get("extensions",{})})
    tris=0;positions=[]
    with (dest/"mesh.bin.bytes").open("wb") as f:
        f.write(struct.pack("<i",len(groups)))
        for g in groups.values():
            v=np.concatenate(g["v"]);n=np.concatenate(g["n"]);uv=np.concatenate(g["uv"]);t=np.concatenate(g["tri"]).flatten()
            positions.append(v);tris+=len(t)//3
            f.write(struct.pack("<ii",len(v),len(t)))
            f.write(np.concatenate([v,n,uv],axis=1).astype("<f4").tobytes());f.write(t.astype("<i4").tobytes())
    (dest/"materials.json").write_text(json.dumps(materials,indent=2))
    record={"name":name,"creator":"Innerscene","license":"CC0-1.0",
        "source":"https://www.innerscene.com/tools/library/3d-parts/"+slug,
        "download":"https://www.innerscene.com/api/library/"+slug+"/download",
        "sha256":hashlib.sha256(data).hexdigest(),"bytes":len(data),"triangles":tris,
        "min":np.concatenate(positions).min(axis=0).tolist(),"max":np.concatenate(positions).max(axis=0).tolist(),
        "changes":"Coordinates converted for Unity; material groups merged; transparent transmission mapped to URP glass. Source GLB retained."}
    records.append(record);print(json.dumps(record))
(root/"SourceAssets/CampusSports/ImportedAssets.json").write_text(json.dumps(records,indent=2))
