using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.Collections.Generic;
using System.Linq;
using Retro;
using UnityEngine.Events;

//(c) 2018 Adam Younis
public class RetroboxEditor : EditorWindow {

    string version = "1.0";

    public SerializedObject mySerializedObject;
    public GUISkin mySkin;
    public RetroboxPrefs preferences;

    //"instance variables"
    public Sheet myTarget;
    private bool targetIsRetroSheet;
    public UnityEngine.Object prevTarget;
    private int selectedFrameIndex = 0;
    private int prevSelectedFrameIndex = 0;
    private List<Texture2D> frameTextures;

    private object copyData;

    //"static variables"
    static UnityEngine.Color spritewindowColour = new Color(0.4f, 0.4f, 0.4f);
    static Texture2D spriteWindowTexture;
    static Rect spriteWindow;
    Rect spriteRect => new Rect(screenSpriteOrigin.x + viewOffset.x, screenSpriteOrigin.y + viewOffset.y, spritePreview.width * spriteScale, spritePreview.height * spriteScale);

    static UnityEngine.Color spriteGridColour = new Color(0.6f, 0.6f, 0.6f);
    static Texture2D spriteGridTexture;
    static Rect spriteGridRect;

    static UnityEngine.Color timelineColour = new Color(0.5f, 0.5f, 0.5f);
    static Texture2D timelineTexture;
    static Rect timelineRect;

    static UnityEngine.Color toolbarColour = new Color(.75f, .75f, .75f);
    static Texture2D toolbarTexture;
    Rect toolbarRect;
    float toolbarH = 48;

    static UnityEngine.Color groupColour = new Color(0.7f, 0.7f, 0.7f);
    static Texture2D groupTexture;
    static Rect groupsRect;
    float groupsW = 260;

    static UnityEngine.Color selectedColour = new Color(0.7f, 0.7f, 0.7f);
    static Texture2D selectedTexture;
    Rect selectedFrameRect;

    static UnityEngine.Color propWindowColour = new Color(0.3f, 0.3f, 0.3f, 0.75f);
    static Texture2D propWindowTexture;
    Rect propWindowRect;
    float margin_ = 10;
    float padding_ = 5;

    //editor variables
    private Vector2 scrollPos = Vector2.zero;
    private Vector2 viewOffset = Vector2.zero;

    float timelineWindowH = 256;
    float spriteScale = 1; //the sprite scale multiplier
    Vector2 screenSpriteOrigin = new Vector2(0, 0); //to position the sprite in the centre of the window

    bool resize = false;
    Rect canvasPartition;
    Group parsingGroup;
    Group selectedGroup;
    Layer parsingLayer;
    Layer selectedLayer;
    FrameData selectedFrameData => selectedLayer.GetFrameData(selectedFrameIndex) != null ? selectedLayer.GetFrameData(selectedFrameIndex) : FrameData.emptyData;
    Layer prevSelectedLayer;
    UnityEvent newSelection;

    Frame parsingFrame => parsingLayer.frames[selectedFrameIndex];
    FrameData parsingFrameData => parsingLayer.GetFrameData(selectedFrameIndex) != null ? parsingLayer.GetFrameData(selectedFrameIndex) : FrameData.emptyData;

    Rect box;
    bool hasKeyFrameHere;
    Texture2D spritePreview;
    bool playing; //simulate animation playback;
    int frameOnPlay;
    double timeAtPlay;
    bool loadedSprites = false;

    Vector2 mouseDownPos;
    Vector2 mouseClickPos;
    Vector2 prevMouseClickPos;

    float mouseDownTime;
    int consecutiveClicks;
    List<KeyValuePair<Group, Layer>> clickedFrames;
    Stack<KeyValuePair<Rect, UnityEditor.MouseCursor>> mouseHandles;

    bool paintedTwice;

    bool shouldRepaint;
    bool expandedProperties;
    float propertiesOffset;
    Group toDelete;

    static Dictionary<string, Color> colours;
    static Dictionary<string, Texture2D> colourTextures;

    Texture2D logo;

    //Icon Textures ---

    //Sprite window icons
    Texture2D zoom1;
    Texture2D zoom2;
    Texture2D zoom4;
    Texture2D zoomFill;
    Texture2D activeZoomTexture;
    Texture2D gridOn;
    Texture2D gridOff;
    Texture2D activeGridTexture;

    //Timeline icons
    Texture2D keyFrame;
    Texture2D emptyFrame;
    Texture2D copyFrame;

    //Layer icons
    Texture2D expand;
    Texture2D collapse;
    Texture2D eyeOpen;
    Texture2D eyeClosed;
    Texture2D trigger;
    Texture2D collider;
    Texture2D noCollision;

    Texture2D isTrigger;

    Texture2D ellipsis;
    Texture2D groupMenu;
    Texture2D arrow;
    Texture2D eye;

    //Toolbar icons
    Texture2D newGroup;
    //Texture2D newLayer;

    Texture2D file;
    Texture2D targetFile;

    //Texture2D properties;
    Texture2D previous;
    Texture2D play;
    Texture2D pause;
    Texture2D next;

    Texture2D frameIcon;
    Texture2D partitionHandle;

    //Selection
    Texture2D frameBorderFirst;
    Texture2D frameBorderMiddle;
    Texture2D frameBorderLast;
    //Texture2D frameBorderSelected;

    Texture2D selectedFrameTexture;
    Texture2D selectedFrameIconBorder;

    Texture2D border;

    bool gridSetting = true;
    int zoomSetting = 1;

    Rect lHandle;
    Rect rHandle;
    Rect tHandle;
    Rect bHandle;
    Rect tlHandle;
    Rect trHandle;
    Rect blHandle;
    Rect brHandle;

    Rect wholeHandle;

    Frame resizingFrame;
    Layer resizingLayer;
    Rect prevBox;    //a mirror of the hitbox currently being edited before we started
    Vector2 prevMouse;    //the position of the mouse at the start of a resize / transform.

    List<BoxProperty> allBoxProperties;
    List<BoxProperty> remainingProperties;
    int propWindowItemCount = 0;
    bool expandedGreyProperties;

    List<BoxProperty> allFrameProperties;
    List<BoxProperty> remainingFrameProperties;
    //int framePropWindowItemCount = 0;

    bool inspectingFrameObject = false;


    enum EditorMode { Default, Move, Play };
    EditorMode mode;

    enum HandleType { Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight, Whole, None };
    HandleType editingHandle;

    [MenuItem("Window/Retrobox Editor")]
    public static void Init() {
        RetroboxEditor window = (RetroboxEditor)GetWindow(typeof(RetroboxEditor));
        window.minSize = new Vector2(150, 450);
        window.Show();
        Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor Default Resources/Retrobox/Images/RetroboxEditor.png");
        GUIContent titleContent = new GUIContent("Retrobox", icon);
        window.titleContent = titleContent;

    }

    void OnFocus() {
        OnEnable();
    }

    private void OnLostFocus() {
        if (myTarget != null) {
            Save();
        }
    }

    void OnEnable() {
        Undo.undoRedoPerformed += UndoCallback;

        LoadTextures();
        LoadPreferences();
        InstantiateEditorTextures();
        SetupEditorInstances();
        SetupBoxProperties();
        SetupFrameProperties();
        LoadSelectedSheet();

    }
    void LoadSelectedSheet() {
        if (Selection.activeObject != null) {
            if (Selection.activeObject.GetType() == typeof(Sheet)) {
                myTarget = (Sheet)Selection.activeObject;
                mySerializedObject = new UnityEditor.SerializedObject(myTarget);

                LoadSprites();
                InitializeRetroSheet();

            }
        }

    }

    void SetupBoxProperties() {
        allBoxProperties = new List<BoxProperty>();
        foreach (KeyValuePair<string, PDataType> kv in preferences.propsDictionary) {
            allBoxProperties.Add(new BoxProperty(kv.Key, preferences.propsDictionary[kv.Key]));
        }
        remainingProperties = allBoxProperties;
        shouldRepaint = true;

    }

    void SetupFrameProperties() {
        //setup frame properties
        allFrameProperties = new List<BoxProperty>();
        foreach (KeyValuePair<string, PDataType> kv in preferences.framePropsDictionary) {
            allFrameProperties.Add(new BoxProperty(kv.Key, preferences.framePropsDictionary[kv.Key]));
        }
        remainingFrameProperties = allFrameProperties;
        shouldRepaint = true;

    }

    void SetupEditorInstances() {
        selectedGroup = null;
        selectedLayer = null;
        resizingFrame = null;
        resizingLayer = null;
        prevSelectedLayer = null;
        selectedFrameIndex = 0;
        prevSelectedFrameIndex = 0;
        newSelection = new UnityEvent();
        newSelection.AddListener(ResetTargetCurvePropertySelection);
        canvasPartition = new Rect(0, 256, 750, 8f);
        paintedTwice = false;
    }

    void LoadTextures() {

        mySkin = (GUISkin)(EditorGUIUtility.Load("Retrobox/Retroskin.guiskin"));
        logo = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RetroBoxEditor_logo.png"));

        newGroup = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons1.png"));
        //newLayer = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons2.png"));

        eyeOpen = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons3.png"));
        eyeClosed = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons4.png"));

        trigger = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons15.png"));
        collider = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons16.png"));
        noCollision = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons21.png"));

        expand = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons9.png"));
        collapse = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons8.png"));

        ellipsis = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons10.png"));
        groupMenu = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons20.png"));


        emptyFrame = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons11.png"));
        keyFrame = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons12.png"));
        copyFrame = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons14.png"));

        previous = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons5.png"));
        play = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons6.png"));
        pause = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons13.png"));
        next = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons7.png"));

        file = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons18.png"));
        targetFile = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons22.png"));

        //properties = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_Icons19.png"));
        partitionHandle = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_windowHandle.png"));


        frameBorderFirst = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_border1.png"));
        frameBorderMiddle = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_border2.png"));
        frameBorderLast = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_border3.png"));

        selectedFrameTexture = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_texture.png"));
        selectedFrameIconBorder = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_selected_frame_icon_border.png"));

        zoomFill = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools1.png"));
        zoom1 = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools2.png"));
        zoom2 = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools3.png"));
        zoom4 = (Texture2D)(EditorGUIUtility.Load("Retrobox/Images/RB_sprite_window_tools4.png"));

    }

    void InstantiateEditorTextures() {

        spriteWindowTexture = new Texture2D(1, 1);
        spriteWindowTexture.SetPixel(0, 0, spritewindowColour);
        spriteWindowTexture.Apply();

        spriteGridTexture = new Texture2D(1, 1);
        spriteGridTexture.SetPixel(0, 0, spriteGridColour);
        spriteGridTexture.Apply();

        toolbarTexture = new Texture2D(1, 1);
        toolbarTexture.SetPixel(0, 0, toolbarColour);
        toolbarTexture.Apply();

        groupTexture = new Texture2D(1, 1);
        groupTexture.SetPixel(0, 0, groupColour);
        groupTexture.Apply();

        timelineTexture = new Texture2D(1, 1);
        timelineTexture.SetPixel(0, 0, timelineColour);
        timelineTexture.Apply();

        selectedTexture = new Texture2D(1, 1);
        selectedTexture.SetPixel(0, 0, selectedColour);
        selectedTexture.Apply();

        propWindowTexture = new Texture2D(1, 1);
        propWindowTexture.SetPixel(0, 0, propWindowColour);
        propWindowTexture.Apply();


    }

    void LoadPreferences() {
        gridSetting = true;
        zoomSetting = 0;
        colours = new Dictionary<string, Color>();
        colourTextures = new Dictionary<string, Texture2D>();
        activeZoomTexture = UpdateActiveZoomTexture();

        preferences = (RetroboxPrefs)(Resources.Load("Retrobox Preferences"));

        if (preferences != null) {
            gridSetting = preferences.cachedGridSetting;
            zoomSetting = preferences.cachedZoomSetting;
            foreach (Shape s in Shape.GetValues(typeof(Shape))) {
                PopulateColourDictionary(preferences.GetShapeDictionary(s));
            }

        } else {
            GeneratePreferences();
        }

    }

    void PopulateColourDictionary(BoxDataDictionary d) {
        for (int i = 0; i < d.Count; i++) {
            Color c = d[i].colour;
            string s = d[i].boxTypeName;
            colours.Add(s, c);
            Texture2D groupColourImage = new Texture2D(16, 16);
            groupColourImage = FillImage(groupColourImage, c);
            groupColourImage.Apply();
            colourTextures.Add(s, groupColourImage);
        }
    }

    private void LoadSprites() {
        loadedSprites = false;
        if (myTarget.spriteList != null) {
            if (myTarget.spriteList.Count > 0) {

                spritePreview = GetTextureFromSprite(myTarget.spriteList[selectedFrameIndex], FilterMode.Point);
                frameTextures = new List<Texture2D>();
                for (int i = 0; i < myTarget.spriteList.Count; i++) {
                    frameTextures.Add(GetTextureFromSprite(myTarget.spriteList[i], FilterMode.Point));
                }

                loadedSprites = true;
            }
        }
    }


    private void InitializeRetroSheet() {
        if (myTarget.spriteList != null) {
            bool setup = false;
            if (myTarget.propertiesList == null) {
                myTarget.propertiesList = new List<Properties>();
                setup = true;
            } else if (myTarget.propertiesList.Count != myTarget.spriteList.Count) {
                setup = true;
            }

            if (setup) {
                for (int i = myTarget.propertiesList.Count; i < myTarget.spriteList.Count; i++) {
                    myTarget.propertiesList.Add(new Properties());
                }

            }
        }
    }

    private void OnSelectionChange() {
        Repaint();
    }

    void Update() {
        if (playing) {
            Repaint();
        }
    }

    //Basically our "update" function... Do all of the things.
    void OnGUI() {
        //Debug.Log(resizingLayer == null);
        preferences = (RetroboxPrefs)(EditorGUIUtility.Load("Retrobox/Resources/Retrobox Preferences.asset"));
        GUI.skin = mySkin;

        //we're actually focused on an animation
        if (prevTarget != Selection.activeObject) OnEnable();
        prevTarget = Selection.activeObject;

        targetIsRetroSheet = (myTarget != null && mySerializedObject != null && loadedSprites && Selection.activeObject != null);
        if (spritePreview == null && targetIsRetroSheet) LoadSprites();

        CalculatePartition();
        canvasPartition = new Rect(canvasPartition.x, this.position.height - timelineWindowH, this.position.width, 8f);

        if (targetIsRetroSheet) mySerializedObject.Update();
        else selectedFrameIndex = 0;

        using (new GUILayout.VerticalScope()) {
            //draw sprite window & frames
            DrawSpriteWindow();

            //draw the properties window
            if (!playing && targetIsRetroSheet) {
                if (selectedLayer != null) {
                    if (selectedLayer.frames != null) { //if we've got a hitbox selected
                        inspectingFrameObject = true;
                        DrawPropertiesWindow("Hitbox Properties", allBoxProperties, selectedLayer.GetFrameData(selectedFrameIndex).props.Values.ToList());

                        if (selectedLayer.kind == Shape.Point) { //and we're on a point curve layer...
                            float curveEditorHeight = 150;
                            DrawCurveEditor(new Rect(margin_, propWindowRect.y + propWindowRect.height + margin_, 250, curveEditorHeight));
                        }

                    }
                } else {
                    inspectingFrameObject = false;
                    DrawPropertiesWindow("Frame Properties", allFrameProperties, myTarget.propertiesList[selectedFrameIndex].frameProperties);

                }

            }

            DrawPartition();
            DrawToolbar();
            DrawTimelineWindow();

        }
        if (targetIsRetroSheet) {
            //  SweepCopyFrames(); //fix any copy frames, updating their values to their respective keyframes.
            HandleInputs(); //handle any user input
            UpdateScale(); //carry out any updates to the scale of the sprite
            DeleteMarkedGroup(); //delete a group we decided to delete during this frame
            CheckNewSelection(); //check for a newly selected hitbox
        }

        if (playing) {
            selectedFrameIndex = (frameOnPlay + (int)((EditorApplication.timeSinceStartup - timeAtPlay) * 10)) % myTarget.spriteList.Count;
            //selectedFrame = ((int)(EditorApplication.timeSinceStartup * 10) % myTarget.spriteList.Count);
        }

        checkDragNDrop();
    }

    Curve targetCurveProperty;
    int editingCurvePoint = -1;
    Rect curvePropRect = new Rect();
    void DrawCurveEditor(Rect rect) {
        curvePropRect = new Rect(rect.min, rect.size);
        Rect dims = new Rect(rect.min, rect.size);
        GUI.DrawTexture(dims, propWindowTexture);
        dims.position += Vector2.one * margin_;
        dims.size -= Vector2.one * margin_ * 2;

        dims.y += dims.height;
        dims.height = -dims.height;
        CaptureCurveInputs(dims, true, false);

        Color prevColour = Handles.color;
        Handles.color *= 2;
        targetCurveProperty.DrawCurve(dims, true, false);
        Handles.color = prevColour;
    }

    void CaptureCurveInputs(Rect bounds, bool handles = true, bool anchors = true) {
        Vector2 o = new Vector2(bounds.x, bounds.y);

        if (targetCurveProperty == null) {
            targetCurveProperty = Curve.Linear;
        } else {

            if (Event.current.type == EventType.MouseDown) {
                if (!playing) {
                    //check each of the points to see if the mouse is within them.
                    int curveHandleIndex = targetCurveProperty.GetEditingHandle(Event.current.mousePosition, bounds, handles, anchors);
                    if (curveHandleIndex != -1) {
                        editingCurvePoint = curveHandleIndex;
                    }
                }
            } else if (Event.current.type == EventType.MouseUp) {
                editingCurvePoint = -1;
            }

            if (editingCurvePoint != -1) {
                //if we find one, set its position to the relative mouse position
                targetCurveProperty.EditCurve(editingCurvePoint, Event.current.mousePosition, bounds);
            }
        }
    }

    void ResetTargetCurvePropertySelection() {
        targetCurveProperty = null;
    }

    //Draw the main window, showing the sprite and frames
    void DrawSpriteWindow() {

        spriteWindow.x = 0;
        spriteWindow.y = 0;
        spriteWindow.width = position.width;
        spriteWindow.height = position.height - timelineWindowH;

        if (preferences == null) {
            NoPreferences();

        } else if (targetIsRetroSheet) {
            DrawCanvas();

        } else if (Selection.activeObject != null) {
            if (Selection.activeObject.GetType() != typeof(Sheet)) {
                Message(Selection.activeObject.name + " is not a Retro Animation");
            } else if (!loadedSprites) {

                Message(
                    "The sprite array for " + myTarget.name + " is empty.",
                    "Drop sprites in to begin."
                );
            }
        } else {
            NothingSelected();
        }

        //Logo
        GUI.DrawTexture(new Rect(spriteWindow.width - logo.width - 12, spriteWindow.height - logo.height - 8, logo.width, logo.height), logo);

    }

    void DrawCanvas() {
        GUILayout.FlexibleSpace();
        using (new GUILayout.HorizontalScope()) {
            GUILayout.FlexibleSpace();

            GUI.DrawTexture(spriteWindow, spriteWindowTexture); //background

            DrawSpriteWindowBackground(spriteRect);
            DrawSprite(spriteRect);
            DrawFrameShapes();

            Event e = Event.current;
            DeselectOnWindowClick(e);
            DrawZoomToggle(e);

            if (mode == EditorMode.Move) {
                EditorGUIUtility.AddCursorRect(spriteWindow, MouseCursor.Pan);
            }

            GUILayout.FlexibleSpace();
        }
        GUILayout.FlexibleSpace();
    }

    //draw the toolbar, which sits between the sprite window and the timeline
    void DrawToolbar() {

        float toolbarY = canvasPartition.y + canvasPartition.height;
        toolbarRect = new Rect(0, toolbarY, position.width, toolbarH);
        GUI.DrawTexture(toolbarRect, toolbarTexture);
        using (new GUILayout.AreaScope(toolbarRect)) {
            using (new GUILayout.HorizontalScope()) {

                Rect toolbarControlsRect = new Rect(toolbarRect.x, 0, groupsW, toolbarRect.height);

                GUI.DrawTexture(toolbarControlsRect, toolbarTexture);
                using (new GUILayout.AreaScope(toolbarControlsRect)) {
                    using (new GUILayout.HorizontalScope()) {

                        GUILayout.Space(3);

                        DrawBurgerMenu();

                        if (targetIsRetroSheet) {
                            DrawTargetFileButton();
                            DrawNewGroupButton();
                            GUILayout.Space(80);
                            DrawPlayerControls();
                            GUILayout.Space(3);
                        } else {
                            GUILayout.Space(groupsW - toolbarH + 3);
                        }
                    }
                }
                DrawTimelineThumbnails();
            }
        }
    }

    void DrawBurgerMenu() {
        //File
        GUI.SetNextControlName("burger");
        if (GUILayout.Button(new GUIContent(file, "Menu"), GUI.skin.GetStyle("HBIcon"))) {
            if (Event.current.button == 0) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("New Retro Sheet"), false, NewRetroSheet);
                menu.AddItem(new GUIContent("Target preferences file"), false, TargetPreferences);
                //menu.AddItem(new GUIContent("Generate preferences"), false, GeneratePreferences);
                menu.AddItem(new GUIContent("Update all sheets"), false, TryUpdate);
                menu.AddItem(new GUIContent("About Retrobox"), false, AboutRetrobox);
                menu.ShowAsContext();
            }
        }

    }

    void DrawTargetFileButton() {
        if (GUILayout.Button(new GUIContent(targetFile, "target file"), GUI.skin.GetStyle("HBIcon"))) {
            EditorGUIUtility.PingObject(myTarget);
        }
    }

    void DrawNewGroupButton() {
        if (GUILayout.Button(new GUIContent(newGroup, "new group"), GUI.skin.GetStyle("HBIcon"))) {
            GenericMenu menu = new GenericMenu();
            foreach (Shape s in Shape.GetValues(typeof(Shape))) {
                menu.AddItem(new GUIContent(s.ToString()), false, AddNewGroup, s);
            }
            menu.ShowAsContext();
        }
    }


    void AddNewGroup(object o) {
        Shape shape = (Shape)o;
        BoxDataDictionary sd = preferences.GetShapeDictionary(shape);
        Group g = new Group(sd.GetKey(0), shape);
        myTarget.groups.Add(g);

        Layer l = new Layer(shape);
        SetupLayer(l);

        g.layers.Add(l);
    }

    void SetupLayer(Layer l) {

        for (int i = 0; i < myTarget.spriteList.Count; i++) {

            if (i == 0) {
                Frame f = new Frame(Frame.Kind.KeyFrame);
                FrameData d = new FrameData();
                d.size = new Vector2(16, 16);
                f.dataId = d.guid;
                d.keyFrameId = f.guid;
                l.frames.Add(f);
                l.frameDataById.Add(d.guid, d);

            } else {
                Frame f = new Frame();
                l.Add(f);
            }
        }
    }

    void DrawPlayerControls() {
        if (GUILayout.Button(new GUIContent(previous, "previous frame"), GUI.skin.GetStyle("HBIcon"))) {  //previous
            if (selectedFrameIndex == 0) {
                selectedFrameIndex = myTarget.spriteList.Count - 1;
            } else {
                selectedFrameIndex = (selectedFrameIndex - 1) % (myTarget.spriteList.Count);
            }
        }

        if (GUILayout.Button((playing) ? new GUIContent(pause, "pause") : new GUIContent(play, "play"), GUI.skin.GetStyle("HBIcon"))) {   //play
            playing = !playing;
            if (playing) {
                frameOnPlay = selectedFrameIndex;
                timeAtPlay = EditorApplication.timeSinceStartup;
            }
        }

        if (GUILayout.Button(new GUIContent(next, "next frame"), GUI.skin.GetStyle("HBIcon"))) {   //next
            selectedFrameIndex = (selectedFrameIndex + 1) % (myTarget.spriteList.Count);
        }
    }

    void DrawTimelineThumbnails() {
        //selected frame goes here
        selectedFrameRect = new Rect(-scrollPos.x + groupsW + (selectedFrameIndex * toolbarH), 0, toolbarH, toolbarH);

        //frameThumbs
        using (new GUILayout.AreaScope(new Rect(groupsW, 0, toolbarRect.width - groupsW, toolbarRect.height))) {
            GUILayout.BeginScrollView(new Vector2(scrollPos.x, 0), false, false, GUIStyle.none, GUIStyle.none, GUILayout.Height(toolbarH), GUILayout.Width(toolbarRect.width - groupsW));

            GUI.DrawTexture(new Rect(selectedFrameIndex * toolbarH, 0, toolbarH, toolbarH), selectedTexture);// draw white background

            if (targetIsRetroSheet) {

                using (new GUILayout.HorizontalScope()) {

                    float hm = toolbarH - 4;
                    for (int i = 0; i < myTarget.spriteList.Count; i++) { //draw frames
                        border = (i == 0) ? frameBorderFirst : frameBorderMiddle; //the first border has a left edge, the others dont to preserve 1px edge.
                        float ratio = (float)(frameTextures[i].height) / (float)(frameTextures[i].width);
                        float x = i * (toolbarH);
                        float y = 2 + (hm - (hm * ratio)) * 0.5f;
                        float w = ratio < 1 ? hm : hm * ratio;
                        float h = ratio < 1 ? hm * ratio : hm;
                        GUI.DrawTexture(new Rect(x, y, w, h), frameTextures[i]); //sprite

                        if (GUILayout.Button(border, GUI.skin.GetStyle("HBIcon"), GUILayout.Width(toolbarH), GUILayout.Height(toolbarH))) { //button
                            if (Event.current.button == 0) {
                                GUI.FocusControl(null);
                                selectedFrameIndex = i;

                            } else if (Event.current.button == 1) {
                                //right click
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Duplicate frame"), false, DuplicateFrame, i);
                                menu.AddItem(new GUIContent("Delete frame"), false, DeleteFrame, i);
                                menu.ShowAsContext();
                            }
                        }
                    }

                }
            }

            GUI.DrawTexture(new Rect(0, 0, position.width, toolbarH), frameBorderLast);
            GUILayout.EndScrollView();
        }
    }

    //draw the background for the timeline
    void DrawTimelineWindowBG() {
        using (new GUILayout.AreaScope(new Rect(0, spriteWindow.height, position.width, position.height - spriteWindow.height))) {

            groupsRect = new Rect(0, toolbarH + canvasPartition.height, groupsW, position.height - spriteWindow.height - toolbarH - canvasPartition.height);
            GUI.DrawTexture(groupsRect, groupTexture);

            timelineRect = new Rect(groupsW, toolbarH + canvasPartition.height, position.width - groupsW, position.height - spriteWindow.height - toolbarH - canvasPartition.height);
            GUI.DrawTexture(timelineRect, timelineTexture);
        }
    }

    //draw the timeline contents
    void DrawTimelineWindow() {
        DrawTimelineWindowBG();

        if (!playing && targetIsRetroSheet) {
            using (new GUILayout.HorizontalScope()) {
                DrawLayersPanel();
                DrawTimeline();
            }
        }

    }

    //Draw the layers on the left side of the screen
    void DrawLayersPanel() {
        float oldScrollx = scrollPos.x;
        scrollPos = GUILayout.BeginScrollView(new Vector2(0, scrollPos.y), false, false, GUIStyle.none, GUIStyle.none, GUILayout.Height(timelineWindowH - toolbarH - canvasPartition.height), GUILayout.Width(groupsW));
        scrollPos.x = oldScrollx;
        using (new GUILayout.VerticalScope(GUILayout.Width(groupsW))) {
            Rect workingGroupRect = new Rect(groupsRect.x, groupsRect.y - toolbarH - canvasPartition.height + propertiesOffset, groupsW, 32);

            float layerWidth = groupsW;//300; //this is a hard variable from the HBLayer style.
            float layerHeight = 30;

            for (int i = 0; i < myTarget.groups.Count; i++) {

                parsingGroup = myTarget.groups[i];
                DrawGroupControl(i, layerWidth, workingGroupRect);
                workingGroupRect.y += (workingGroupRect.height);

                if (parsingGroup.expanded) {

                    workingGroupRect.y += padding_;
                    GUILayout.Space(padding_); //top margin per group

                    //draw the frames
                    for (int j = 0; j < parsingGroup.layers.Count; j++) { // for each layer in the group

                        if (parsingGroup.layers[j].kind == Shape.Box) {
                            DrawLayerControl(j, workingGroupRect, layerHeight);
                        } else {
                            DrawPointControl(j, workingGroupRect, layerHeight);

                        }
                        workingGroupRect.y += layerHeight;
                        workingGroupRect.y += padding_;

                        GUILayout.Space(padding_);
                    }
                }
            }
        }
        GUILayout.EndScrollView();
    }

    void DrawPointControl(int layerIndex, Rect rect, float layerHeight) {
        using (new GUILayout.HorizontalScope(GUILayout.Height(layerHeight + padding_))) {
            //control set goes here

            GUILayout.Space(padding_); //layer margin left

            //draw ellipsis
            if (GUILayout.Button(new GUIContent(ellipsis, "layer options"), GUI.skin.GetStyle("HBIcon"), GUILayout.Width(25))) {
                /*
                if (Event.current.button == 0) {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add layer below"), false, AddLayerToGroup, myTarget.groups.IndexOf(currentGroup));
                    menu.AddItem(new GUIContent("Delete layer"), false, DeleteLayer, currentLayer);

                    menu.ShowAsContext();
                }
                */
            }
            GUILayout.Label("Curve ");
            GUILayout.Space(padding_);
        }
    }

    void DrawGroupControl(int groupIndex, float w, Rect rect) {
        Group group = myTarget.groups[groupIndex];
        using (new GUILayout.HorizontalScope(GUI.skin.GetStyle("HBLayer"))) {
            // GUILayout.Button(new Texture2D(1, 1), GUI.skin.GetStyle("HBIcon"));

            try {
                //draw the colour strip
                GUI.DrawTexture(new Rect(0, rect.y, 30, 30), colourTextures[group.myBoxType]);
            } catch (System.Exception e) {
                Debug.LogError("The active Retrobox Preferences file does not contain an entry for '" + group.myBoxType + "'. " + e);
            }

            if (GUILayout.Button(new GUIContent(groupMenu, "Group options"), GUI.skin.GetStyle("HBIcon"), GUILayout.Width(30))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add layer to group"), false, AddLayerToGroup, groupIndex);
                menu.AddItem(new GUIContent("Delete group"), false, DeleteGroup, group);
                menu.ShowAsContext();
            }

            if (GUILayout.Button(new GUIContent(group.myBoxType, "set box type"), GUI.skin.GetStyle("Label"), GUILayout.Width(110))) {
                GenericMenu menu = new GenericMenu();
                for (int j = 0; j < preferences.boxDictionary.Count; j++) {

                    List<object> parameters = new List<object>();
                    parameters.Add(group);
                    parameters.Add(preferences.boxDictionary.GetKey(j));
                    menu.AddItem(new GUIContent(preferences.boxDictionary.GetKey(j)), false, SetGroupType, parameters);

                }
                menu.ShowAsContext();
            }

            GUILayout.Space(w - 100 - padding_ * 25 - 10); //middle portion //layer label spacer

            if (GUILayout.Button((group.visible) ? new GUIContent(eyeOpen, "hide group") : new GUIContent(eyeClosed, "show group"), GUI.skin.GetStyle("HBIcon"))) { //draw visibility
                Undo.RecordObject(myTarget, "set visibility to " + !group.visible);
                group.visible = !(group.visible);
            }

            GUIContent content = new GUIContent();
            switch (group.collisionType) {
                case Group.CollisionType.Collider:
                    content = new GUIContent(collider, "collider");
                    break;
                case Group.CollisionType.Trigger:
                    content = new GUIContent(trigger, "trigger");
                    break;
                case Group.CollisionType.NoCollide:
                    content = new GUIContent(noCollision, "no collider");
                    break;
            }

            if (GUILayout.Button(content, GUI.skin.GetStyle("HBIcon"))) { //draw isTrigger
                switch (group.collisionType) {
                    case Group.CollisionType.Collider:
                        Undo.RecordObject(myTarget, "set trigger to" + group.collisionType);
                        group.collisionType = Group.CollisionType.Trigger;
                        break;
                    case Group.CollisionType.Trigger:
                        Undo.RecordObject(myTarget, "set trigger to" + group.collisionType);
                        group.collisionType = Group.CollisionType.NoCollide;
                        break;
                    case Group.CollisionType.NoCollide:
                        Undo.RecordObject(myTarget, "set trigger to" + group.collisionType);
                        group.collisionType = Group.CollisionType.Collider;
                        break;
                }
            }

            if (GUILayout.Button((group.expanded) ? new GUIContent(collapse, "collapse group") : new GUIContent(expand, "expand group"), GUI.skin.GetStyle("HBIcon"))) { //draw collapsed
                Undo.RecordObject(myTarget, "change expanded status to " + !group.expanded);
                group.expanded = !(group.expanded);
            }
        }


        if (RightClicked(rect)) {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add layer to group"), false, AddLayerToGroup, groupIndex);
            menu.AddItem(new GUIContent("Delete group"), false, DeleteGroup, parsingGroup);
            menu.ShowAsContext();
        }


    }


    void DrawLayerControl(int layerIndex, Rect rect, float layerHeight) {
        //float x, y, w, h;
        using (new GUILayout.HorizontalScope(GUILayout.Height(layerHeight))) {

            GUILayout.Space(padding_); //layer margin left

            //draw ellipsis
            if (GUILayout.Button(new GUIContent(ellipsis, "layer options"), GUI.skin.GetStyle("HBIcon"), GUILayout.Width(25))) {
                if (Event.current.button == 0) {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add layer below"), false, AddLayerToGroup, myTarget.groups.IndexOf(parsingGroup));
                    menu.AddItem(new GUIContent("Delete layer"), false, DeleteLayer, parsingLayer);

                    menu.ShowAsContext();
                }
            }

            GUILayout.Space(padding_);

            //draw active keyframe
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Hitbox " + (layerIndex + 1));

        }
        if (RightClicked(rect)) {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete group"), false, DeleteGroup, parsingGroup);
            menu.ShowAsContext();
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown && !rect.Contains(e.mousePosition)) {
            GUI.FocusControl(null);
        }

    }
    //Draw the Timeline
    void DrawTimeline() {
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Height(timelineWindowH - toolbarH - canvasPartition.height));

        using (new GUILayout.VerticalScope(GUILayout.Width(position.width - groupsW - 16))) {
            propertiesOffset = (expandedProperties) ? 72 : 0; //are properties open?

            float workingtimelineHeight = propertiesOffset + 10; //offset between diamonds and row bars
            GUILayout.Space(padding_);

            DrawFrameFocus();

            using (new GUILayout.VerticalScope()) {
                for (int i = 0; i < myTarget.groups.Count; i++) { //for every group

                    workingtimelineHeight += 32;
                    GUILayout.Space(32); //top margin per group
                    parsingGroup = myTarget.groups[i];

                    if (parsingGroup.expanded) { //if group is visible

                        workingtimelineHeight += padding_; //margin per expanded group
                        GUILayout.Space(padding_);

                        for (int j = 0; j < parsingGroup.layers.Count; j++) { //for every layer in every group
                            DrawTimelineLayer(parsingGroup.layers[j], workingtimelineHeight);
                            workingtimelineHeight += 35;

                        }

                    }
                }
            }
        }
        GUILayout.EndScrollView();
    }


    void DrawFrameFocus() {
        selectedFrameRect = new Rect(selectedFrameIndex * toolbarH, scrollPos.y, toolbarH + 1, timelineRect.height);
        GUI.DrawTexture(selectedFrameRect, selectedFrameTexture);

    }

    bool followingKeyframe;
    void DrawTimelineLayer(Layer l, float workingtimelineHeight) {
        followingKeyframe = false;
        parsingLayer = l;
        using (new GUILayout.HorizontalScope(GUILayout.Height(20))) {
            GUILayout.Space(2); //left margin per group
            for (int k = 0; k < myTarget.spriteList.Count; k++) { //for every hitbox in every layer in every group...
                DrawTimelineFrame(k, workingtimelineHeight);
            }
        }
        GUILayout.Space(15);
    }

    void DrawTimelineFrame(int index, float workingtimelineHeight) {
        Frame f = parsingLayer.frames[index];
        if (index < myTarget.spriteList.Count - 1) { //if we're not the 2nd last frame, draw a line onwards from us.
            if (f.kind == Frame.Kind.KeyFrame || (f.kind == Frame.Kind.CopyFrame && followingKeyframe)) {
                try {
                    //set the colour we'll be using
                    GUI.DrawTexture(new Rect((toolbarH * 0.5f) + (index * toolbarH), workingtimelineHeight, toolbarH, 9), colourTextures[parsingGroup.myBoxType]);
                } catch (System.Exception e) {
                    Debug.LogError("The active Retrobox Preferences file does not contain an entry for '" + parsingGroup.myBoxType + "'. " + e);
                }
            }
        }

        if (f.kind == Frame.Kind.KeyFrame) {//draw the correct keyframe icon
            frameIcon = keyFrame;
            followingKeyframe = true;
        } else if (f.kind == Frame.Kind.Empty) {
            frameIcon = emptyFrame;
            followingKeyframe = false;
        } else if (f.kind == Frame.Kind.CopyFrame) {
            frameIcon = copyFrame;
        }

        if (parsingGroup == selectedGroup && parsingLayer == selectedLayer && index == selectedFrameIndex) {//BORDER if we are the selected frame icon
            DrawFrameSelection(index, workingtimelineHeight);
        }

        if (GUILayout.Button(frameIcon, GUI.skin.GetStyle("HBIcon"), GUILayout.Width(toolbarH))) {//display the keyframe icon
            selectedGroup = parsingGroup;
            selectedLayer = parsingLayer;
            selectedFrameIndex = index;
        }
    }

    void DrawFrameSelection(int index, float workingtimelineHeight) {
        Frame f = parsingLayer.frames[index];
        if (GUI.Button(new Rect(-2 + (index * 48), workingtimelineHeight - 12, 52, 32), selectedFrameIconBorder, GUI.skin.GetStyle("HBIcon"))) {//keyframe icon border
            if (f.kind == Frame.Kind.KeyFrame) {
                SetEmptyFrame(index);

            } else if (f.kind == Frame.Kind.Empty) {
                SetCopyFrame(index);

            } else if (f.kind == Frame.Kind.CopyFrame) {
                SetKeyFrame(index);
            }
        }
    }

    void SetEmptyFrame(int i) {
        Undo.RecordObject(myTarget, "change frame to empty frame");
        Frame f = parsingLayer.frames[i];
        //if we're a keyframe, remove our data from the FrameDataDictionary
        parsingLayer.RemoveKeyFrameData(f);
        //make us empty
        f.kind = Frame.Kind.Empty;
        f.dataId = System.Guid.Empty.ToString();

        //resync the sheet
        parsingLayer.ResyncFrames(i);
    }

    void SetKeyFrame(int index) {
        Undo.RecordObject(myTarget, "change frame to key frame");
        //Assumes this was a copy frame
        Frame f = parsingLayer.frames[index];
        f.kind = Frame.Kind.KeyFrame;

        FrameData d;
        if (f.dataId != null && parsingLayer.frameDataById.ContainsKey(f.dataId)) {
            d = FrameData.Clone(parsingLayer.frameDataById[f.dataId]);
        } else {
            d = new FrameData();
            d.size = new Vector2(16, 16);
        }
        d.keyFrameId = f.guid;
        f.dataId = d.guid;
        parsingLayer.frameDataById.Add(d.guid, d);
        parsingLayer.ResyncFrames(index);

    }

    void SetCopyFrame(int index) {

        Undo.RecordObject(myTarget, "change frame to copy frame");
        Frame f = parsingLayer.frames[index];
        f.dataId = index > 0 ? parsingLayer.frames[index - 1].dataId : System.Guid.Empty.ToString();
        f.kind = Frame.Kind.CopyFrame;
        if (parsingLayer.GetPreviousKeyFrame(index) != null) {
            parsingLayer.ResyncFrames(parsingLayer.frames.IndexOf(parsingLayer.GetPreviousKeyFrame(index)));
        }

    }

    //Draw window handle
    private void DrawPartition() {
        GUI.DrawTexture(canvasPartition, EditorGUIUtility.whiteTexture);
        GUI.DrawTexture(new Rect((canvasPartition.x + canvasPartition.width * 0.5f) - 7, canvasPartition.y, 14, canvasPartition.height), partitionHandle);
        EditorGUIUtility.AddCursorRect(canvasPartition, MouseCursor.ResizeVertical);

    }

    private void CalculatePartition() {
        if (Event.current.type == EventType.MouseDown && canvasPartition.Contains(Event.current.mousePosition)) {
            resize = true;
        }
        if (resize && Event.current.mousePosition.y > 100 && Event.current.mousePosition.y < this.position.height - 100) {
            timelineWindowH = this.position.height - Event.current.mousePosition.y;
            canvasPartition.Set(0, Event.current.mousePosition.y, position.width, canvasPartition.height);
        }
        if (Event.current.type == EventType.MouseUp) {
            resize = false;

        }
    }

    void DrawSpriteWindowBackground(Rect spriteRect) {
        GUI.DrawTexture(spriteRect, selectedTexture); //sprite background
        if (gridSetting) DrawGrid(spriteRect);
    }

    void DrawSprite(Rect spriteRect) {
        using (new GUILayout.HorizontalScope()) {
            GUI.DrawTexture(new Rect(spriteRect.x, spriteRect.y, spriteRect.width, spriteRect.height), spritePreview, ScaleMode.ScaleToFit); //our sprite
        }
    }

    //Draw the frames in the editor
    void DrawFrameShapes() {
        bool selectingNonEmptyFrame = IsALayerSelected() && !selectedLayer.frames[selectedFrameIndex].IsEmpty();
        clickedFrames = new List<KeyValuePair<Group, Layer>>();
        mouseHandles = new Stack<KeyValuePair<Rect, MouseCursor>>();

        //if we've gone to the trouble of selecting a layer already... Draw that one first
        // if (IsALayerSelected() && selectingNonEmptyFrame) {
        //    DrawFrame(selectedGroup, selectedLayer);
        //}

        for (int i = myTarget.groups.Count - 1; i >= 0; i--) { //for every group in the sheet
            parsingGroup = myTarget.groups[i]; //current group = index...

            if (parsingGroup.visible) { //if we can see the group
                for (int j = parsingGroup.layers.Count - 1; j >= 0; j--) { //for every layer in the group
                    parsingLayer = parsingGroup.layers[j];
                    //if we're not the selected layer, draw us now. 


                    //Debug.Log(parsingLayer.frames);
                    //Debug.Log(!IsThisLayerSelected(parsingGroup, parsingLayer));
                    //Debug.Log(parsingLayer.frames[selectedFrame].isValid);

                    if (
                    !LayerIsSelected(parsingGroup, parsingLayer) &&
                    !parsingLayer.frames[selectedFrameIndex].IsEmpty()) {
                        DrawFrame(parsingGroup, parsingLayer, true);
                    }

                }
            }
        }

        //draw the selected layer again since handles read front-back while canvas is drawn back-front.
        if (IsALayerSelected() && selectingNonEmptyFrame) {
            DrawFrame(selectedGroup, selectedLayer);
        }

        int c = mouseHandles.Count;
        for (int d = 0; d < c; d++) {
            KeyValuePair<Rect, MouseCursor> kv = mouseHandles.Pop();
            EditorGUIUtility.AddCursorRect(kv.Key, kv.Value);
        }

        CheckClickedObjects();
    }

    void CheckClickedObjects() {

        if (clickedFrames.Count > 0) { //if we clicked at least one frame
            int max = clickedFrames.Count - 1;
            int index = consecutiveClicks - 1;
            int modIndex = index % clickedFrames.Count;
            selectedGroup = clickedFrames[max - modIndex].Key; //get the nth clicked group
            selectedLayer = clickedFrames[max - modIndex].Value;//get the nth clicked layer

        } else if (ClickedRect(spriteWindow)) {//we clicked empty space, deselect
            if (!ClickedRect(propWindowRect) && !ClickedRect(curvePropRect)) {
                selectedGroup = null;
                selectedLayer = null;
            }
        }
    }

    void DrawFrame(Group group, Layer layer, bool visible = true) {

        switch (layer.kind) {
            case Shape.Box:
                DrawHitbox(group, layer, visible);
                break;
            case Shape.Point:
                DrawPoint(group, layer, visible);
                break;
        }
    }

    void DeselectOnWindowClick(Event e) {
        if (e.type == EventType.MouseDown && spriteWindow.Contains(e.mousePosition)) {
            GUI.FocusControl(null);
        }

    }

    void DrawZoomToggle(Event e) {
        using (new GUILayout.AreaScope(new Rect(8, spriteWindow.height - (32 + 8), (32 * 2) + (8 * 2), 32))) {
            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button(new GUIContent(activeZoomTexture, "cycle zoom setting"), GUIStyle.none)) {
                    if (e.button == 0) {
                        zoomSetting++;
                        if (zoomSetting > 3) zoomSetting = 0;
                        switch (zoomSetting) {
                            case 0:
                                activeZoomTexture = zoomFill;
                                viewOffset.x = 0;
                                viewOffset.y = 0;
                                break;
                            case 1:
                                activeZoomTexture = zoom1;
                                break;
                            case 2:
                                activeZoomTexture = zoom2;
                                break;
                            case 3:
                                activeZoomTexture = zoom4;
                                break;
                        }
                    }
                }

            }
        }
    }

    void DrawPoint(Group group, Layer layer, bool visible = true) {
        SetHandleColour(group, layer);
        Vector2 size = Vector2.one * spriteScale;

        FrameData d = layer.GetFrameData(selectedFrameIndex);
        Rect box = d.rect;
        box = FrameToCanvasSpace(box);

        Handles.DrawSolidRectangleWithOutline(new Rect(box.position - (size * 0.5f), size * 0.5f), Handles.color, Color.clear); //top
        Handles.DrawWireCube((Vector3)box.position, Vector3.one * 16);

        Frame startFrame = layer.GetCurrentKeyFrameOrPrevious(selectedFrameIndex);
        Frame nextKeyFrame = layer.GetNextKeyFrame(selectedFrameIndex);

        if (startFrame != null && nextKeyFrame != null) {
            DrawLayerCurve(group, layer, startFrame, nextKeyFrame, spriteRect, LayerIsSelected(group, layer), true);
        }


    }

    int layerEditingCurvePoint = -1;
    void DrawLayerCurve(Group group, Layer layer, Frame startFrame, Frame endFrame, Rect canvasBounds, bool drawHandles = true, bool drawAnchors = true) {
        int startIndex = layer.frames.IndexOf(startFrame);
        int endIndex = layer.frames.IndexOf(endFrame);
        FrameData startData = layer.frameDataById[startFrame.dataId];
        FrameData endData = layer.frameDataById[endFrame.dataId];

        Rect spriteSpaceBounds = new Rect(Vector2.zero, myTarget.spriteList[selectedFrameIndex].rect.size);

        Curve c = new Curve(startData.position, startData.forwardHandle, endData.backHandle, endData.position);

        //check each of the points to see if the mouse is within them.
        int curveHandleIndex = c.GetEditingHandle(Event.current.mousePosition, spriteSpaceBounds, canvasBounds, drawHandles, drawAnchors);

        if (Clicked() && curveHandleIndex != -1) {
            clickedFrames.Add(new KeyValuePair<Group, Layer>(group, layer));
        }

        //Edit curve
        if (Event.current.type == EventType.MouseDown) {
            if (!playing) {
                if (curveHandleIndex != -1) {
                    layerEditingCurvePoint = curveHandleIndex;
                }
            }
        } else if (Event.current.type == EventType.MouseUp) {
            layerEditingCurvePoint = -1;
        }

        if (drawHandles) {

            if (layerEditingCurvePoint != -1) {

                //if we find one, set its position to the relative mouse position
                c.EditCurve(layerEditingCurvePoint, Event.current.mousePosition, spriteSpaceBounds, canvasBounds, false);
                //resizingFrame = parsingFrame;//we are now editing this hitbox only
                //resizingLayer = parsingLayer;
                startData.position = c.a;
                startData.forwardHandle = c.a1;
                endData.backHandle = c.b1;
                endData.position = c.b;
            }

        }
        //start.curve.DebugMousePos(Event.current.mousePosition, spriteSpaceBounds, canvasBounds);
        c.DrawCurve(spriteSpaceBounds, canvasBounds, drawHandles, drawAnchors);

        //Draw target property Curve along this curve, if one exists.
        if (targetCurveProperty != null) {
            DrawPropertyCurveAlongLayerCurve(targetCurveProperty, layer, startIndex, endIndex, spriteSpaceBounds, canvasBounds);
        }
    }

    void DrawPropertyCurveAlongLayerCurve(Curve curveProp, Layer layer, int startIndex, int endIndex, Rect spriteSpaceBounds, Rect canvasBounds) {
        //if we're looking at a curve
        int duration = endIndex - startIndex;

        //loop through all of the frames from the start to the end of the curve.
        for (int i = 0; i < duration; i++) {
            //one frame ahead of this, just because frame 0 is kind of useless.
            float curveTime = curveProp.EvaluateForX((float)(i + 1) / (float)duration).y;

            Curve frameCurve = layer.CurveFromKeyFrames(startIndex);

            //draw the points along that curve, one for each frame
            Vector2 markerPos = frameCurve.EvaluateDrawnCurve(spriteSpaceBounds, canvasBounds, curveTime);
            Color prevColour = Handles.color;

            Handles.color = (startIndex + i) == selectedFrameIndex ? prevColour * 4 : prevColour * 3;
            //if the current frame is selected, make that one a bit brighter
            Vector2 size = Vector2.one * 8;
            Rect r = new Rect(markerPos - (size * 0.5f), size * 0.5f);
            Handles.DrawSolidRectangleWithOutline(r, Handles.color, Color.clear);

            Handles.color = prevColour;
        }
    }

    //draw a hitbox
    void DrawHitbox(Group group, Layer layer, bool visible = true) {
        SetHandleColour(group, layer);
        SetupHitboxBounds(layer);

        //setup grabby handles
        int h = 6;
        int h2 = 3;
        SetupHandles(h);

        //is the mouse over a handle?
        if (Event.current.type == EventType.MouseDown && !playing) {
            if (LayerIsSelected(group, layer)) {
                DetermineSelectedHandle(group, layer);
            }

        }

        //make the edit
        bool shouldResize =
            layer.frames.IndexOf(resizingFrame) == selectedFrameIndex &&
            Event.current.type == EventType.MouseDrag &&
            !mode.Equals(EditorMode.Move) &&
            !resize;

        if (shouldResize) {
            ResizeHitbox(group, layer);
        }

        if (visible) {
            DrawHandles(group, layer, h, h2);
        }
        //did we click this box?
        if (ClickedRect(box)) {
            clickedFrames.Add(new KeyValuePair<Group, Layer>(parsingGroup, parsingLayer));
        }
    }

    void SetupHitboxBounds(Layer layer) {
        //adjust size to match screen.
        box = layer.GetFrameData(selectedFrameIndex).rect;
        box = FrameToCanvasSpace(box);

        //rounding...
        box.x = (int)box.x;
        box.y = (int)box.y;
        box.width = (int)box.width;
        box.height = (int)box.height;
    }

    void SetupHandles(int h) {
        int h2 = h * 2;
        tHandle = new Rect((box.x + box.width * .5f) - h, box.y - h, h2, h2); //top
        bHandle = new Rect((box.x + box.width * .5f) - h, box.y + box.height - h, h2, h2); //bottom
        lHandle = new Rect(box.x - h, (box.y + box.height * .5f) - h, h2, h2); //left
        rHandle = new Rect(box.x + box.width - h, (box.y + box.height * .5f) - h, h2, h2); //right

        tlHandle = new Rect(box.x - h, box.y - h, h2, h2); //topleft
        trHandle = new Rect((box.x + box.width) - h, box.y - h, h2, h2); //topright
        blHandle = new Rect(box.x - h, box.y + box.height - h, h2, h2); //bottom
        brHandle = new Rect((box.x + box.width) - h, box.y + box.height - h, h2, h2); //bottom

        wholeHandle = new Rect(box.x, box.y, box.width, box.height); //All
    }

    void SetHandleColour(Group group, Layer layer) {
        try {
            //set the colour we'll be using
            Handles.color = (!IsALayerSelected() || LayerIsSelected(group, layer)) ?
            colours[group.myBoxType] :
            Handles.color = spritewindowColour;
        } catch (System.Exception e) {
            Debug.LogError("The active Retrobox Preferences file does not contain an entry for '" + group.myBoxType + "'. " + e);
        }
    }

    void DetermineSelectedHandle(Group group, Layer layer) {
        editingHandle = GetEditingHandle(Event.current.mousePosition);

        if (editingHandle == HandleType.None && wholeHandle.Contains(Event.current.mousePosition)) {
            editingHandle = HandleType.Whole;
        }

        if (editingHandle != HandleType.None) {
            resizingFrame = layer.frames[selectedFrameIndex];//we are now editing this hitbox only
            resizingLayer = layer;
            FrameData fd = resizingLayer.GetFrameData(selectedFrameIndex);
            prevBox = new Rect(fd.rect.x, fd.rect.y, fd.rect.width, fd.rect.height);//where we started
            prevMouse = Event.current.mousePosition;
        }
    }

    void DrawHandles(Group group, Layer layer, int h, int h2) {
        //draw hitbox lines
        Handles.DrawAAPolyLine(3, new Vector3(box.x, box.y), new Vector3(box.x + box.width, box.y));//top
        Handles.DrawAAPolyLine(3, new Vector3(box.x, box.y + box.height), new Vector3(box.x + box.width, box.y + box.height));//bottom
        Handles.DrawAAPolyLine(3, new Vector3(box.x + box.width, box.y), new Vector3(box.x + box.width, box.y + box.height));//right
        Handles.DrawAAPolyLine(3, new Vector3(box.x, box.y), new Vector3(box.x, box.y + box.height));//left

        if (LayerIsSelected(group, layer) && layer.frames[selectedFrameIndex].IsKeyFrame() && !playing) {
            //draw filled in box
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x, box.y, box.width, box.height), new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.16f), Color.clear); //top

            //draw handles
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width / 2 - h2, box.y - h2, h, h), Handles.color, Color.clear); //top
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width / 2 - h2, box.y + box.height - h2, h, h), Handles.color, Color.clear); //bottom
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x - h2, box.y + box.height / 2 - h2, h, h), Handles.color, Color.clear); //left
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width - h2, box.y + box.height / 2 - h2, h, h), Handles.color, Color.clear); //right
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x - h2, box.y - h2, h, h), Handles.color, Color.clear); //tl
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width - h2, box.y - h2, h, h), Handles.color, Color.clear); //tr
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x - h2, box.y + box.height - h2, h, h), Handles.color, Color.clear); //bl
            Handles.DrawSolidRectangleWithOutline(new Rect(box.x + box.width - h2, box.y + box.height - h2, h, h), Handles.color, Color.clear); //br


            //draw mouse icons
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(wholeHandle, MouseCursor.MoveArrow)); //all
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(lHandle, MouseCursor.ResizeHorizontal)); //left
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(tHandle, MouseCursor.ResizeVertical)); //top
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(rHandle, MouseCursor.ResizeHorizontal)); //right
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(bHandle, MouseCursor.ResizeVertical)); //bottom
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(tlHandle, MouseCursor.ResizeUpLeft));//left
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(trHandle, MouseCursor.ResizeUpRight)); //top
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(blHandle, MouseCursor.ResizeUpRight)); //right
            mouseHandles.Push(new KeyValuePair<Rect, MouseCursor>(brHandle, MouseCursor.ResizeUpLeft)); //bottom

        }
    }

    void ResizeHitbox(Group group, Layer layer) {
        selectedGroup = group;
        selectedLayer = layer;
        Vector2 offset = (Event.current.mousePosition - screenSpriteOrigin - viewOffset) / spriteScale;
        Vector2 newPos = new Vector2(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y));

        FrameData c = layer.GetFrameData(selectedFrameIndex);
        Undo.RecordObject(myTarget, "resize hitbox");
        if (editingHandle == HandleType.Left) {
            c.position = new Vector2(newPos.x, c.position.y);
            float x = (prevBox.width + (prevBox.x - c.rect.x));
            c.size = new Vector2(x, c.size.y);

        } else if (editingHandle == HandleType.Right) {

            float x = (newPos.x - prevBox.x);
            c.size = new Vector2(x, c.size.y);

        } else if (editingHandle == HandleType.Top) {
            float height = (prevBox.height + (prevBox.y - newPos.y));
            c.position = new Vector2(c.position.x, newPos.y);
            c.size = new Vector2(c.size.x, height);

        } else if (editingHandle == HandleType.Bottom) {
            c.size = new Vector2(c.size.x, newPos.y - prevBox.y);

        } else if (editingHandle == HandleType.TopLeft) {
            c.position = newPos;
            c.size = prevBox.size + (prevBox.position - c.position);

        } else if (editingHandle == HandleType.TopRight) {
            c.position = new Vector2(c.position.x, newPos.y);
            c.size = new Vector2(newPos.x - prevBox.x, prevBox.height + (prevBox.y - c.rect.y));

        } else if (editingHandle == HandleType.BottomLeft) {
            c.position = new Vector2(newPos.x, c.position.y);
            c.size = new Vector2(prevBox.width + (prevBox.x - c.rect.x), newPos.y - prevBox.y);

        } else if (editingHandle == HandleType.BottomRight) {
            c.size = newPos - prevBox.position;

        } else if (editingHandle == HandleType.Whole) {
            c.position = prevBox.position + (Event.current.mousePosition - prevMouse) / spriteScale;
            c.position = new Vector2((int)c.position.x, (int)c.position.y);
        }
    }

    HandleType GetEditingHandle(Vector2 mPos) {
        HandleType h = HandleType.None;
        if (lHandle.Contains(mPos)) {
            h = HandleType.Left;
        } else if (rHandle.Contains(mPos)) {
            h = HandleType.Right;
        } else if (tHandle.Contains(mPos)) {
            h = HandleType.Top;
        } else if (bHandle.Contains(mPos)) {
            h = HandleType.Bottom;
        } else if (tlHandle.Contains(mPos)) {
            h = HandleType.TopLeft;
        } else if (trHandle.Contains(mPos)) {
            h = HandleType.TopRight;
        } else if (blHandle.Contains(mPos)) {
            h = HandleType.BottomLeft;
        } else if (brHandle.Contains(mPos)) {
            h = HandleType.BottomRight;
        }
        return h;
    }

    void DrawGrid(Rect r) { //what a doozy...

        Rect gr = new Rect(r.x, r.y, 16 * spriteScale, 16 * spriteScale); //define a single 16x16 tile

        for (int i = 0; i < spritePreview.width / 16; i++) { //iterate over X

            for (int j = 0; j < spritePreview.height / 16; j += 2) { //iterate over 
                GUI.DrawTexture(gr, spriteGridTexture); //draw box
                gr.y += (gr.height * 2); //increment y axis
            }

            gr.height = r.y + r.height - gr.y; //prepare for last box "remainder" height
            if (r.y + r.height - gr.y > 0) GUI.DrawTexture(gr, spriteGridTexture); //draw last box in column
            gr.height = 16 * spriteScale; //reset for new column
            gr.y = (i % 2 != 0) ? r.y : r.y + gr.height; //reset the entire y axis
            gr.x += (gr.width); //increment x axis

        }

        //Additional column if necessary
        if (r.x + r.width - gr.x > 0) {
            gr.width = r.x + r.width - gr.x; //prepare for last boxes "remainder" width

            for (int j = 0; j < spritePreview.height / 16; j += 2) { //iterate over Y
                GUI.DrawTexture(gr, spriteGridTexture); //draw box
                gr.y += (gr.height * 2); //increment y axis
            }

            gr.height = r.y + r.height - gr.y; //prepare for last box "remainder" height (AND width)
            if (r.y + r.height - gr.y > 0) GUI.DrawTexture(gr, spriteGridTexture); //draw last box in column
                                                                                   //gr.height = 16 * spriteScale; //reset for new column
        }

    }

    void CalculatePropertiesWindow(List<BoxProperty> allProperties, List<BoxProperty> selectedProperties) {
        remainingProperties = new List<BoxProperty>();
        foreach (BoxProperty sProp in allProperties) { //list off all of the properties
            bool found = false;
            foreach (BoxProperty cProp in selectedProperties) { //check for instances of those properties
                if (cProp.name.Equals(sProp.name)) {
                    found = true;
                }
            }
            if (!found) remainingProperties.Add(sProp); //if we didn't find one, tack it on at the end as a greyed out property
        }

        propWindowItemCount = selectedProperties.Count;
        if (expandedGreyProperties) {
            propWindowItemCount += remainingProperties.Count;
        }
    }

    void DrawPropertiesWindow(string propertiesName, List<BoxProperty> allProperties, List<BoxProperty> selectedProperties) {

        CalculatePropertiesWindow(allProperties, selectedProperties);

        int basePropertyRowsCount = 3;
        propWindowRect = new Rect(margin_, margin_, 250, (propWindowItemCount + basePropertyRowsCount) * 21 + 50);
        float w = propWindowRect.width - 50;

        GUI.DrawTexture(propWindowRect, propWindowTexture);

        Rect pwr = new Rect(propWindowRect.x + margin_, propWindowRect.y + margin_, propWindowRect.width - (margin_ * 2), propWindowRect.height - (margin_ * 2));

        using (new GUILayout.AreaScope(pwr)) {
            using (new EditorGUILayout.VerticalScope()) {
                EditorGUILayout.LabelField(propertiesName, EditorStyles.boldLabel);
                GUILayout.Space(margin_); //gap

                if (selectedLayer != null) {
                    DrawFixedProperties(w);
                }

                BoxProperty p;
                for (int i = 0; i < selectedProperties.Count; i++) { //for each instance of a property that we have...
                    p = selectedProperties[i];
                    DrawProperty(w, p, true); //draw it
                }

                bool prevExpanded = expandedGreyProperties;
                expandedGreyProperties = EditorGUILayout.Foldout(expandedGreyProperties, "more properties");

                if (expandedGreyProperties) {
                    for (int i = 0; i < remainingProperties.Count; i++) { //for each of the remaining grey properties...
                        p = remainingProperties[i];
                        DrawProperty(w, p, false); //draw it.
                    }

                }

                if (prevExpanded != expandedGreyProperties) {
                    shouldRepaint = true;
                }
            }
        }

        if (!paintedTwice) {
            paintedTwice = true;
            shouldRepaint = true;
        }
        paintedTwice = false;

    }

    void DrawFixedProperties(float width) {
        float field = width * 0.5f;

        using (new GUILayout.HorizontalScope()) {
            EditorGUILayout.LabelField("position", GUILayout.Width(field));
            Undo.RecordObject(myTarget, "changed position");
            selectedFrameData.position = EditorGUILayout.Vector2Field("", selectedFrameData.position, GUILayout.Width(field));
        }

        using (new GUILayout.HorizontalScope()) {
            EditorGUILayout.LabelField("size", GUILayout.Width(field));
            Undo.RecordObject(myTarget, "changed size");
            selectedFrameData.size = EditorGUILayout.Vector2Field("", selectedFrameData.size, GUILayout.Width(field));
        }
    }


    void DrawProperty(float width, BoxProperty property, bool active) {
        GUI.enabled = active;
        float field = width * 0.5f;
        using (new GUILayout.HorizontalScope()) {

            EditorGUILayout.LabelField(property.name, GUILayout.Width(field));
            //EditorGUILayout.LabelField(p.dataType.ToString(), GUILayout.Width(w * 0.2f));

            switch (property.dataType) { //draw the property

                case PDataType.Bool:
                    Undo.RecordObject(myTarget, "change " + property.name);
                    property.boolVal = EditorGUILayout.Toggle(active ? property.boolVal : false, GUILayout.Width(field));
                    break;

                case PDataType.String:
                    Undo.RecordObject(myTarget, "change " + property.name);
                    property.stringVal = EditorGUILayout.TextField(active ? property.stringVal : "", GUILayout.Width(field));
                    break;

                case PDataType.Int:
                    Undo.RecordObject(myTarget, "change " + property.name);
                    property.intVal = EditorGUILayout.IntField(active ? property.intVal : 0, GUILayout.Width(field));
                    break;

                case PDataType.Float:
                    Undo.RecordObject(myTarget, "change " + property.name);
                    property.floatVal = EditorGUILayout.FloatField(active ? property.floatVal : 0, GUILayout.Width(field));
                    break;

                case PDataType.Vector2:
                    Undo.RecordObject(myTarget, "change " + property.name);
                    property.vectorVal = EditorGUILayout.Vector2Field("", active ? property.vectorVal : new Vector2(), GUILayout.Width(field));
                    break;
                case PDataType.Curve:
                    //just default to the first curve property that exists
                    if (targetCurveProperty == null) {
                        targetCurveProperty = property.curveVal;
                    }
                    //disable the button if we're already selecting it.
                    GUI.enabled = targetCurveProperty != property.curveVal;
                    Undo.RecordObject(myTarget, "change " + property.name);
                    if (GUILayout.Button(GUI.enabled ? "target" : "inspecting")) {
                        targetCurveProperty = property.curveVal;
                    }
                    GUI.enabled = true;
                    break;
            }

            if (active) { //List controls for this property (remove, add)
                Undo.RecordObject(myTarget, "delete property " + property.name);
                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20))) {
                    if (inspectingFrameObject) {
                        selectedLayer.GetFrameData(selectedFrameIndex).props.Remove(property.name);
                    } else {
                        myTarget.propertiesList[selectedFrameIndex].frameProperties.Remove(property);
                    }
                    shouldRepaint = true;
                }
            } else {
                GUI.enabled = true;
                Undo.RecordObject(myTarget, "added property " + property.name);
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20))) {
                    if (inspectingFrameObject) {
                        selectedLayer.GetFrameData(selectedFrameIndex).props.Add(property.name, new BoxProperty(property.name, property.dataType));
                    } else {
                        myTarget.propertiesList[selectedFrameIndex].frameProperties.Add(new BoxProperty(property.name, property.dataType));
                    }
                    shouldRepaint = true;

                }
            }
        }
        GUI.enabled = true;
    }

    void AddNewProperty(object o) {
        string s = (string)o;
        Undo.RecordObject(myTarget, "added new property");
        selectedLayer.GetFrameData(selectedFrameIndex).props.Add(s, new BoxProperty(s, preferences.propsDictionary[s]));
    }

    void UpdateScale() { //set the zoom scale of the canvas
        spriteScale = GetScaleFromZoom(zoomSetting);
        screenSpriteOrigin.x = (spriteWindow.width * 0.5f) - (spritePreview.width * 0.5f * spriteScale);
        screenSpriteOrigin.y = (spriteWindow.height * 0.5f) - (spritePreview.height * 0.5f * spriteScale);

        //handle the canvas view bounds X
        if (viewOffset.x > spritePreview.width * spriteScale * 0.5f) viewOffset.x = spritePreview.width * spriteScale * 0.5f;
        if (viewOffset.x < -spritePreview.width * spriteScale * 0.5f) viewOffset.x = -spritePreview.width * spriteScale * 0.5f;

        //handle the canvas view bounds Y
        if (viewOffset.y > spritePreview.height * spriteScale * 0.5f) viewOffset.y = spritePreview.height * spriteScale * 0.5f;
        if (viewOffset.y < -spritePreview.height * spriteScale * 0.5f) viewOffset.y = -spritePreview.height * spriteScale * 0.5f;

    }

    public float GetScaleFromZoom(int zoom) {
        switch (zoom) {
            case 0:
                return (spriteWindow.height - 20) / myTarget.spriteList[0].rect.height;
            case 1:
                return 2;
            case 2:
                return 4;
            case 3:
                return 8;
        }
        return 2;
    }

    public Texture2D UpdateActiveZoomTexture() {
        switch (zoomSetting) {
            case 0:
                return zoomFill;
            case 1:
                return zoom1;
            case 2:
                return zoom2;
            case 3:
                return zoom4;
        }
        return zoomFill;
    }

    //Handle a bunch of miscelaneous code... mostly inputs but also resetting the sprite preview.
    //I'll fix this at some point...
    void HandleInputs() {
        Event e = Event.current; //the event being handled
        mode = (e.alt || e.button == 2) ? EditorMode.Move : EditorMode.Default; //mode selection

        if (mode == EditorMode.Move) {
            shouldRepaint = true; //repaint every frame if we're moving
            viewOffset += e.delta * 0.5f; //offset is centred(?)
        }

        HandleBoxResizeEnd(e);
        HandleKeyPresses(e);
        HandleMouseEvents(e);
        HandleScrollToZoom(e);

        if (selectedFrameIndex != prevSelectedFrameIndex) {// if we've changed frames in the last frame, update.
            spritePreview = frameTextures[selectedFrameIndex];
        }

        if (shouldRepaint) {
            shouldRepaint = false;
            Repaint();
            activeZoomTexture = UpdateActiveZoomTexture();
        }
    }

    void HandleMouseEvents(Event e) {
        if (e.type == EventType.MouseDown && e.button == 0) { //left mouse click down
            mouseDownPos = new Vector2(e.mousePosition.x, e.mousePosition.y);
            mouseDownTime = Time.time;
        }

        if (e.type == EventType.MouseUp && e.button == 0 && Vector2.Distance(e.mousePosition, mouseDownPos) < 3 && Time.time - mouseDownTime < 0.2f) { //left mouse click up
            mouseClickPos = new Vector2(e.mousePosition.x, e.mousePosition.y);
            consecutiveClicks = (Vector2.Distance(mouseClickPos, prevMouse) == 0) ? consecutiveClicks + 1 : 1; //handle consecutive clicks down to click through layers
            prevMouse = new Vector2(mouseClickPos.x, mouseClickPos.y);
            shouldRepaint = true;

        } else {
            mouseClickPos = new Vector2(-1000, -1000);
            if (Vector2.Distance(prevMouse, e.mousePosition) > 0) consecutiveClicks = 0;
        }

    }

    void HandleScrollToZoom(Event e) {
        int prevZoomSetting = zoomSetting;

        //scroll to cycle through zoom layers
        if (e.type == EventType.ScrollWheel) {
            if (zoomSetting == 0) {
                zoomSetting = Mathf.Clamp((int)(((spriteWindow.height - 20) / (myTarget.spriteList[0].rect.height)) * 0.5f), 0, 3);
                shouldRepaint = true;
            }

            if (e.delta.y > 0) { //scrolling down = zooming out
                if (zoomSetting > 1) {
                    zoomSetting--;
                    shouldRepaint = true;
                }
            } else { //scrolling up = zooming in
                if (zoomSetting < 3) {
                    zoomSetting++;
                    shouldRepaint = true;
                }
            }
        }

        if (prevZoomSetting != zoomSetting && zoomSetting != 0) {
            UpdateViewOffsetWithScale(
                e.mousePosition,
                GetScaleFromZoom(zoomSetting),
                GetScaleFromZoom(prevZoomSetting)
            );
        }
    }

    void HandleBoxResizeEnd(Event e) {

        if (e.type == EventType.MouseUp) { //finish resizing a hitbox
            if (resizingLayer != null) {
                FrameData r = resizingLayer.GetFrameData(selectedFrameIndex);
                if (resizingFrame.IsKeyFrame() || resizingFrame.IsCopyFrame()) {

                    //just in case we happened to have resized one and leave it with negative dimensions, adjust them
                    if (r.rect.width < 0) {
                        r.size = new Vector2(Mathf.Abs(r.rect.width), r.size.y);
                        r.position = new Vector2(r.position.x - r.rect.width, r.position.y);
                    }
                    if (r.rect.height < 0) {
                        r.size = new Vector2(r.size.x, Mathf.Abs(r.rect.height));
                        r.position = new Vector2(r.position.x, r.position.y - r.rect.height);
                    }
                }

                shouldRepaint = true;
                resizingFrame = null;
                resizingLayer = null;
            }
        }
    }

    void HandleKeyPresses(Event e) {
        mode = (e.alt || e.button == 2) ? EditorMode.Move : EditorMode.Default; //mode selection
        int prevZoomSetting = zoomSetting;
        if (e.type == EventType.KeyDown) { //key presses...

            shouldRepaint = true; //repaint the canvas if we press a button

            switch (e.keyCode) {
                case KeyCode.RightArrow: //next frame
                    selectedFrameIndex = (selectedFrameIndex + 1) % (myTarget.spriteList.Count);
                    break;

                case KeyCode.LeftArrow: //previous frame
                    if (selectedFrameIndex > 0) selectedFrameIndex--;
                    else selectedFrameIndex = myTarget.spriteList.Count - 1;
                    break;

                case KeyCode.BackQuote:  //Fit
                    zoomSetting = 0;
                    viewOffset.x = 0;
                    viewOffset.y = 0;
                    shouldRepaint = true;
                    break;

                case KeyCode.Alpha1:  //1x
                    zoomSetting = 1;
                    shouldRepaint = true;
                    break;

                case KeyCode.Alpha2:  //2x
                    zoomSetting = 2;
                    shouldRepaint = true;
                    break;

                case KeyCode.Alpha3:  //3x
                    zoomSetting = 3;
                    shouldRepaint = true;
                    break;

                case KeyCode.G:  //show grid
                    gridSetting = !gridSetting;
                    break;

                case KeyCode.H: //show/hide all layers
                    ShowHideToggle();
                    break;

                case KeyCode.Space: //play animation
                    playing = !playing;
                    if (playing) {
                        frameOnPlay = selectedFrameIndex;
                        timeAtPlay = EditorApplication.timeSinceStartup;
                    }
                    break;

                case KeyCode.Escape:  //unselect frame
                    selectedGroup = null;
                    selectedLayer = null;
                    break;

                case KeyCode.S: //save (for those of us with OCD)
                    if (Event.current.modifiers == EventModifiers.Control) {
                        Save();
                    }
                    break;

                case KeyCode.C: //Copy
                    if (Event.current.modifiers == EventModifiers.Control) {
                        Copy();
                    }
                    break;

                case KeyCode.V: //Paste
                    if (Event.current.modifiers == EventModifiers.Control) {
                        Paste();
                    }
                    break;
            }

        }
    }


    public void UpdateViewOffsetWithScale(Vector2 mPos, float scale, float prevScale) { //adjust our canvas offset to be anchored by the mouse position as we scale.
                                                                                        //find the screen position of the pixel under the mouse
        Vector2 pixelFromSpriteOrigin = new Vector2(((Event.current.mousePosition.x - screenSpriteOrigin.x - viewOffset.x) / spriteScale), ((Event.current.mousePosition.y - screenSpriteOrigin.y - viewOffset.y) / spriteScale));
        Vector2 spriteOrigin = screenSpriteOrigin + viewOffset;
        Vector2 pixelScreenPos = spriteOrigin + (pixelFromSpriteOrigin * prevScale);

        //scale the sprite
        viewOffset *= scale / prevScale;

        //find the new screen position of the pixel
        Vector2 newScreenSpriteOrigin = new Vector2((spriteWindow.width * 0.5f) - (spritePreview.width * 0.5f * scale), (spriteWindow.height * 0.5f) - (spritePreview.height * 0.5f * scale));
        Vector2 newSpriteOrigin = newScreenSpriteOrigin + viewOffset;
        Vector2 newPixelScreenPos = newSpriteOrigin + (pixelFromSpriteOrigin * scale);

        //translate the sprite by the difference of those two positions.
        viewOffset += pixelScreenPos - newPixelScreenPos;
    }


    void ShowHideToggle() { //show or hide all groups in the sheet.
        bool foundVisible = false;
        foreach (Group g in myTarget.groups) {
            if (g.visible) {
                foundVisible = true;
                break;
            }
        }

        Undo.RecordObject(myTarget, "show / hide all groups");
        foreach (Group g in myTarget.groups) {
            g.visible = !foundVisible; //if we found one on, you're all going off. Otherwise all turn on.
        }
    }


    void Message(params string[] strings) {
        using (new GUILayout.AreaScope(spriteWindow)) {

            GUI.DrawTexture(spriteWindow, spriteWindowTexture); //background
            GUI.DrawTexture(new Rect(spriteWindow.width * 0.5f - 200f, spriteWindow.height * 0.5f - 100f, 400, 200), groupTexture); //notice background

            GUILayout.FlexibleSpace();
            foreach (string s in strings) {
                using (new GUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(s);
                    GUILayout.FlexibleSpace();
                }
            }
            GUILayout.FlexibleSpace();

        }
    }

    void NoPreferences() {
        string msg = "Could not find preferences file in /Resources. \n \nMove an existing file to /Resources to continue \nor generate a new one below.";
        MessageWithButton(
            "New Preferences!",
            GeneratePreferences,
            msg
        );
    }

    void NothingSelected() {
        MessageWithButton(
            "New Sheet!",
            NewRetroSheet,
            "Nothing Selected...",
            "Create a new animation at this location?"
        );

    }



    void MessageWithButton(string buttonLabel, System.Action action, params string[] strings) {
        using (new GUILayout.AreaScope(spriteWindow)) {

            GUI.DrawTexture(spriteWindow, spriteWindowTexture); //background
            GUI.DrawTexture(new Rect(spriteWindow.width * 0.5f - 200f, spriteWindow.height * 0.5f - 100f, 400, 200), groupTexture); //notice background

            GUILayout.FlexibleSpace();
            using (new GUILayout.VerticalScope()) {
                foreach (string s in strings) {
                    using (new GUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(s);
                        GUILayout.FlexibleSpace();
                    }
                }
                GUILayout.Space(margin_);

                using (new GUILayout.HorizontalScope()) {

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(buttonLabel)) {
                        action.Invoke();
                    }
                    GUILayout.FlexibleSpace();
                }

            }
            GUILayout.FlexibleSpace();

        }
    }



    bool IsALayerSelected() {
        return (selectedGroup != null && selectedLayer != null);
    }

    bool LayerIsSelected(Group group, Layer layer) {
        return (selectedGroup == group && selectedLayer == layer);
    }

    public bool Clicked() {
        return (mouseClickPos.x != -1000 && mouseClickPos.y != -1000);
    }

    //Did we click inside the given rectangle?
    public bool ClickedRect(Rect r) {
        return (Clicked() && r.Contains(Event.current.mousePosition));
    }

    //Did we right-click inside the given rectangle?
    public bool RightClicked(Rect r) {
        Event e = Event.current;
        if (e.type == EventType.ContextClick) {
            Vector2 mousepos = e.mousePosition;

            if (r.Contains(mousepos)) {
                e.Use();
                return true;
            }

        }
        return false;
    }



    //Insert a frame (o = int)
    void DuplicateFrame(object intObject) {
        int i = (int)intObject;
        Undo.RecordObject(myTarget, "duplicate frame");

        //add the new frame
        myTarget.spriteList.Insert(i, myTarget.spriteList[i]);
        //clone frame properties
        myTarget.propertiesList.Insert(i, new Properties(myTarget.propertiesList[i].frameProperties));

        for (int x = 0; x < myTarget.groups.Count; x++) { //iterate through groups
            for (int y = 0; y < myTarget.groups[x].layers.Count; y++) { //and layers
                //clone the frame object
                Frame prevFrame = myTarget.groups[x].layers[y].frames[i];
                Frame newFrame = Frame.Clone(prevFrame);
                myTarget.groups[x].layers[y].Add(newFrame, i + 1);
            }
        }

        selectedFrameIndex = i + 1;
        LoadSprites();
    }

    //Delete a frame (o = int)
    void DeleteFrame(object intObject) {
        int i = (int)intObject;

        if (i < myTarget.spriteList.Count && i >= 0) {
            Undo.RecordObject(myTarget, "remove frame");
            myTarget.spriteList.Remove(myTarget.spriteList[i]);
            myTarget.propertiesList.Remove(myTarget.propertiesList[i]);

            for (int x = 0; x < myTarget.groups.Count; x++) {//for all groups
                for (int y = 0; y < myTarget.groups[x].layers.Count; y++) { //for all layers
                    myTarget.groups[x].layers[y].Remove(i);
                }
            }
        }

        if (selectedFrameIndex >= myTarget.spriteList.Count) {
            selectedFrameIndex = myTarget.spriteList.Count - 1;
        }
        LoadSprites();
    }

    //Add a layer to selected group (o = int)
    void AddLayerToGroup(object o) {
        int i = (int)o;
        Layer l = new Layer(myTarget.groups[i].layerKind);
        SetupLayer(l);
        myTarget.groups[i].layers.Add(l);
    }

    //Delete a group (o = Group)
    void DeleteGroup(object o) {
        Group group = (Group)o;
        toDelete = group;
    }

    //Delete a group (public group "toDelete")
    void DeleteMarkedGroup() {
        if (myTarget.groups.Contains(toDelete)) {
            Undo.RecordObject(myTarget, "remove group");
            toDelete.layers.Clear();
            myTarget.groups.Remove(toDelete);

            Save();

        }
    }


    //Delete a layer (o = Layer)
    void DeleteLayer(object o) {
        Layer layer = (Layer)o;
        for (int i = 0; i < myTarget.groups.Count; i++) {
            Undo.RecordObject(myTarget, "remove layer");
            myTarget.groups[i].layers.Remove(layer);
        }
    }

    //Set the group type to a given enum value (o = list of parameters)
    void SetGroupType(object o) {
        Undo.RecordObject(myTarget, "change group type");
        List<object> p = (List<object>)o;
        Group g = (Group)p[0];
        string s = (string)p[1];
        myTarget.groups[myTarget.groups.IndexOf(g)].myBoxType = s;

    }

    //Fill an image with a colour...
    Texture2D FillImage(Texture2D img, Color col) {
        for (int i = 0; i < img.width; i++) {
            for (int j = 0; j < img.height; j++) {
                img.SetPixel(i, j, col);
            }
        }
        return img;
    }

    /// <summary>
    /// Return the individual texture for a given index in a sprite matrix
    /// </summary>
    /// <param name="sprite">The source sprite.</param>
    /// <param name="filterMode">The desired filter mode.</param>
    public static Texture2D GetTextureFromSprite(Sprite sprite, FilterMode filterMode) {
        var rect = sprite.rect;
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        tex.filterMode = filterMode;
        Graphics.CopyTexture(sprite.texture, 0, 0, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, tex, 0, 0, 0, 0);
        tex.Apply(true);
        return tex;
    }
    public Rect FrameToCanvasSpace(Rect rect) {
        rect.x = (rect.x * spriteScale) + screenSpriteOrigin.x + viewOffset.x;
        rect.y = (rect.y * spriteScale) + screenSpriteOrigin.y + viewOffset.y;
        rect.width *= spriteScale;
        rect.height *= spriteScale;
        return rect;
    }

    //DRAG AND DROP FUNCTIONALITY
    public void checkDragNDrop() {
        switch (Event.current.type) {

            case EventType.MouseDown:
                // reset the DragAndDrop Data
                DragAndDrop.PrepareStartDrag();
                break;

            case EventType.DragUpdated:

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                break;

            case EventType.DragPerform:

                DragAndDrop.AcceptDrag();
                TryImporting(DragAndDrop.objectReferences);
                break;

            case EventType.MouseDrag:
                //DragAndDrop.StartDrag("Dragging");
                Event.current.Use();
                break;

            case EventType.MouseUp:

                // Clean up, in case MouseDrag never occurred:
                DragAndDrop.PrepareStartDrag();
                break;

            case EventType.DragExited:
                break;
        }
    }


    public void Save() {
        EditorUtility.SetDirty(myTarget);
        AssetDatabase.SaveAssets();
    }

    public void Copy() {
        if (selectedGroup != null && selectedLayer != null) { //layer
            Frame frameToCopy = new Frame();
            int groupIndex = myTarget.groups.IndexOf(selectedGroup);
            int layerIndex = myTarget.groups[groupIndex].layers.IndexOf(selectedLayer);

            frameToCopy = myTarget.groups[groupIndex].layers[layerIndex].frames[selectedFrameIndex];
            copyData = Frame.Clone(frameToCopy);

        } else { //frame?

        }
    }


    public void Paste() {
        if (selectedGroup != null && selectedLayer != null) {//layer
            if (copyData is Frame frameToPaste) { //if our copydata is a hitbox
                int groupIndex = myTarget.groups.IndexOf(selectedGroup);
                int layerIndex = myTarget.groups[groupIndex].layers.IndexOf(selectedLayer);
                Undo.RecordObject(myTarget, "pasted hitbox data");
                Layer l = myTarget.groups[groupIndex].layers[layerIndex];
                l.Remove(selectedFrameIndex);
                l.Add(frameToPaste, selectedFrameIndex);
                //myTarget.groups[groupIndex].layers[layerIndex].frames[selectedFrame] = new Frame(selectedLayer, Frame.Kind.KeyFrame, frameToPaste.position, frameToPaste.size);
                //myTarget.groups[groupIndex].layers[layerIndex].frames[selectedFrame].props = frameToPaste.props;
            }
        } else {

        }
    }

    private void TryImporting(Object[] objects) { //import sprites "dropped" onto the editor
        for (int i = 0; i < objects.Length; i++) {
            if (objects[i] is Sprite) {
                if (myTarget.spriteList == null) {
                    myTarget.spriteList = new List<Sprite>();
                    myTarget.propertiesList = new List<Properties>();
                    myTarget.groups = new List<Group>();

                } else {
                    foreach (Group g in myTarget.groups) {
                        foreach (Layer l in g.layers) {
                            l.frames.Add(new Frame());
                        }
                    }
                }

                myTarget.spriteList.Add((Sprite)objects[i]);
                myTarget.propertiesList.Add(new Properties());

                LoadSprites();
                InitializeRetroSheet();
            }

        }
    }

    private void CheckNewSelection() {
        if (selectedLayer != null) {
            if (prevSelectedLayer != selectedLayer ||
                prevSelectedFrameIndex != selectedFrameIndex ||
                selectedLayer.GetFrameData(prevSelectedFrameIndex) != selectedLayer.GetFrameData(selectedFrameIndex)
            ) {
                newSelection.Invoke();
            }
        }
        prevSelectedLayer = selectedLayer;
        prevSelectedFrameIndex = selectedFrameIndex;
    }

    private void TargetPreferences() {
        EditorGUIUtility.PingObject(preferences);
        Selection.activeObject = preferences;
    }

    private void NewRetroSheet() {
        Sheet asset = CreateInstance<Sheet>();
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
          asset.GetInstanceID(),
          CreateInstance<EndRetroNameEdit>(),
          string.Format("{0}.asset", "RetroSheet"),
          AssetPreview.GetMiniThumbnail(asset),
          null);

    }

    private void GeneratePreferences() {

        RetroboxPrefs asset = CreateInstance<RetroboxPrefs>();

        asset.boxDictionary = new BoxDataDictionary();
        asset.pointDictionary = new BoxDataDictionary();

        asset.propsDictionary = new BoxPropertiesDictionary();
        asset.framePropsDictionary = new BoxPropertiesDictionary();

        asset.cachedZoomSetting = 0;
        asset.cachedGridSetting = true;

        //default box types
        asset.boxDictionary.Add("Physics", new BoxData(new Color(28 / 256f, 219 / 256f, 149 / 256f), "Physics", 0, true, Retro.Shape.Box));
        asset.boxDictionary.Add("Hurt", new BoxData(new Color(242 / 256f, 206 / 256f, 4 / 256f), "Hurt", 8, false, Retro.Shape.Box));
        asset.boxDictionary.Add("Hit", new BoxData(new Color(255 / 256f, 58 / 256f, 48 / 256f), "Hit", 9, false, Retro.Shape.Box));

        AssetDatabase.CreateAsset(asset, "Assets/Editor Default Resources/Retrobox/Resources/Retrobox Preferences.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        /*ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
          asset.GetInstanceID(),
          CreateInstance<EndRetroNameEdit>(),
          string.Format("{0}.asset", "Retrobox Preferences"),
          AssetPreview.GetMiniThumbnail(asset),
          null); 
          */
    }

    private void TryUpdate() {
        int updated = 0;
        string[] sheetReferences = AssetDatabase.FindAssets("t:Sheet");
        Sheet[] sheets = new Sheet[sheetReferences.Length];
        for (int i = 0; i < sheetReferences.Length; i++) {

            sheets[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(sheetReferences[i]), typeof(Sheet)) as Sheet;
            //1.0a no longer supported

            if (sheets[i].GetVersion().Equals("1.0")) {//find old version...(1.0a)

                foreach (Group g in sheets[i].groups) {
                    foreach (Layer l in g.layers) {

                        for (int f = 0; f < l.frames.Count; f++) {

                            if (l.frames[f].IsKeyFrame()) {


                                //l.ResyncFrames(f);
                            }
                        }
                    }
                }

                updated++;//we did something

                EditorUtility.SetDirty(sheets[i]);
                sheets[i].SetVersion(this, version); //do something
            }

        }
        Save();
        Debug.Log(sheetReferences.Length + " total sheets found, " + updated + " old sheets updated to latest version (" + version + ")");

    }

    void UndoCallback() {
        Repaint();
    }

    void AboutRetrobox() {
        Debug.Log("Retrobox " + myTarget.GetVersion() + ".\nCreated by Adam Younis @ Uppon Hill.\nContact: support@upponhill.com");
    }

}

internal class EndRetroNameEdit : EndNameEditAction {
    #region implemented abstract members of EndNameEditAction
    public override void Action(int instanceId, string pathName, string resourceFile) {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
    }
    #endregion
}