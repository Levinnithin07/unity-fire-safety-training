using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
public static class SetGameView1080
{
 public static string Run(){
 const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 var assembly=typeof(Editor).Assembly;var viewType=assembly.GetType("UnityEditor.GameView");
 var view=EditorWindow.GetWindow(viewType);var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
 var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance",flags).GetValue(null);
 var groupType=viewType.GetProperty("currentSizeGroupType",flags).GetValue(view);
 var group=sizesType.GetMethod("GetGroup",flags).Invoke(singleton,new[]{groupType});
 var g=group.GetType();int total=(int)g.GetMethod("GetTotalCount",flags).Invoke(group,null);int chosen=-1;
 for(int i=0;i<total;i++){
 var size=g.GetMethod("GetGameViewSize",flags).Invoke(group,new object[]{i});var t=size.GetType();
 if((int)t.GetProperty("width",flags).GetValue(size)==1920&&(int)t.GetProperty("height",flags).GetValue(size)==1080){chosen=i;break;}
 }
 if(chosen<0){var sizeType=assembly.GetType("UnityEditor.GameViewSize");var kind=assembly.GetType("UnityEditor.GameViewSizeType");
 var size=Activator.CreateInstance(sizeType,flags,null,new object[]{Enum.Parse(kind,"FixedResolution"),1920,1080,"Training 1080p"},null);
 g.GetMethod("AddCustomSize",flags).Invoke(group,new[]{size});chosen=total;}
 viewType.GetProperty("selectedSizeIndex",flags).SetValue(view,chosen);view.Show();view.Repaint();
 return "Game view set to fixed 1920x1080, index "+chosen;
 }
}
