using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

internal class EndNameEdit : EndNameEditAction {
    #region implemented abstract members of EndNameEditAction
    public override void Action(int instanceId, string pathName, string resourceFile) {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
    }

    #endregion
}

/// <summary>
/// Scriptable object window.
/// </summary>
public class ScriptableObjectWindow : EditorWindow {
    Vector2 scrollPos;
    private int selectedIndex;
    //private string[] names;

    private static Type[] types;

    private static string[] names;


    private static Type[] Types {
        get { return types; }
        set {
            types = value;
            names = types.Select(t => t.FullName).ToArray();
        }
    }

    public static void Init(Type[] scriptableObjects) {
        Types = scriptableObjects;

        var window = EditorWindow.GetWindow<ScriptableObjectWindow>(true, "Create a new ScriptableObject", true);
        window.ShowPopup();
    }

    /// <summary>
    /// Returns the assembly that contains the script code for this project (currently hard coded)
    /// </summary>
    private static Assembly GetAssembly() {
        return Assembly.Load(new AssemblyName("Assembly-CSharp"));
    }

    [MenuItem("Window/Scriptable Object Window")]
    public static void Init() {
        var assembly = GetAssembly();

        // Get all classes derived from ScriptableObject
        var allScriptableObjects = (from t in assembly.GetTypes()
                                    where t.IsSubclassOf(typeof(ScriptableObject))
                                    select t).OrderBy(x => x.Name).ToArray();


        Types = allScriptableObjects;

        ScriptableObjectWindow window = (ScriptableObjectWindow)GetWindow(typeof(ScriptableObjectWindow));
        window.minSize = new Vector2(50, 100);
        window.Show();
        GUIContent titleContent = new GUIContent("Scriptables");
        window.titleContent = titleContent;
    }

    public void OnGUI() {
        if (names == null) {
            Init();
        }
        /*
		GUILayout.Label("ScriptableObject Class");
		selectedIndex = EditorGUILayout.Popup(selectedIndex, names);

		if (GUILayout.Button("Create"))
		{
			var asset = ScriptableObject.CreateInstance(types[selectedIndex]);
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				asset.GetInstanceID(),
				ScriptableObject.CreateInstance<EndNameEdit>(),
				string.Format("{0}.asset", names[selectedIndex]),
				AssetPreview.GetMiniThumbnail(asset), 
				null);

			Close();
		}
		*/
        //        EditorGUILayout.BeginVertical();
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < names.Length; i++) {
            if (GUILayout.Button(names[i])) {

                var asset = ScriptableObject.CreateInstance(types[i]);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    asset.GetInstanceID(),
                    ScriptableObject.CreateInstance<EndNameEdit>(),
                    string.Format("{0}.asset", names[i]),
                    AssetPreview.GetMiniThumbnail(asset),
                    null);

            }
        }
        GUILayout.EndScrollView();

        // EditorGUILayout.BeginVertical();

        /*
        if (GUILayout.Button("DataLibrary")) {
            DataLibrary asset = CreateInstance<DataLibrary>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "DataLibrary"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("BoxSheet")) {
            Retro.Sheet asset = ScriptableObject.CreateInstance<Retro.Sheet>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "BoxSheet"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("EventSequence")) {
            EventSequence asset = ScriptableObject.CreateInstance<EventSequence>();
            asset.Setup();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                ScriptableObject.CreateInstance<EndNameEdit>(),
                string.Format("{0}.asset", "EventSequence"),
                AssetPreview.GetMiniThumbnail(asset),
                null);

        }

        if (GUILayout.Button("DialogueCharacter")) {
            DialogueCharacter asset = ScriptableObject.CreateInstance<DialogueCharacter>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                ScriptableObject.CreateInstance<EndNameEdit>(),
                string.Format("{0}.asset", "DialogueCharacter"),
                AssetPreview.GetMiniThumbnail(asset),
                null);

        }
        if (GUILayout.Button("Item")) {
            Insignia.Item asset = ScriptableObject.CreateInstance<Insignia.Item>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "Item"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("Component")) {
            ComponentMaterial asset = ScriptableObject.CreateInstance<ComponentMaterial>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "ComponentMaterial"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("Accessory")) {
            Insignia.Accessory asset = ScriptableObject.CreateInstance<Insignia.Accessory>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "Item"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }


        if (GUILayout.Button("Design")) {
            Design asset = ScriptableObject.CreateInstance<Design>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "Design"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("Location")) {
            Location asset = ScriptableObject.CreateInstance<Location>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "Location"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("Quest")) {
            Quest asset = ScriptableObject.CreateInstance<Quest>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "Quest"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("QuestFlag")) {
            QuestFlag asset = ScriptableObject.CreateInstance<QuestFlag>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "QuestFlag"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("Ability")) {
            Ability asset = ScriptableObject.CreateInstance<Ability>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "Ability"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("SceneInstanceCollection")) {
            SceneInstanceCollection asset = ScriptableObject.CreateInstance<SceneInstanceCollection>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "SceneInstanceCollection"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("AudioClipMixer")) {
            AudioClipMixer asset = ScriptableObject.CreateInstance<AudioClipMixer>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "AudioClipMixer"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }

        if (GUILayout.Button("Layermask Profile")) {
            LayerMaskProfile asset = ScriptableObject.CreateInstance<LayerMaskProfile>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "LayerMaskProfile"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("EffectLayer")) {
            PixelFX.Layer asset = ScriptableObject.CreateInstance<PixelFX.Layer>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "EffectLayer"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("EffectLayerList")) {
            PixelFX.LayerList asset = ScriptableObject.CreateInstance<PixelFX.LayerList>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "EffectLayerList"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
        if (GUILayout.Button("ForgeProcess")) {
            ForgeProcess asset = ScriptableObject.CreateInstance<ForgeProcess>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
              asset.GetInstanceID(),
              ScriptableObject.CreateInstance<EndNameEdit>(),
              string.Format("{0}.asset", "ForgeProcess"),
              AssetPreview.GetMiniThumbnail(asset),
              null);

        }
*/
    }
}