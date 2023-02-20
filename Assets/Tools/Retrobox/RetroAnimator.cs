using System.Collections;
using UnityEngine;
using UnityEngine.Events;


namespace Retro {
    public class BoxCollisionEvent : UnityEvent<Collision> { }
    public class HitConfirmEvent : UnityEvent<CollisionInfo> { }

    public class RetroAnimator : MonoBehaviour {

        string version = "1.0";
        [HideInInspector]
        public RetroboxPrefs preferences;

        //public objects
        public Rigidbody2D rigidBody;
        public SpriteRenderer spriteRenderer;
        public Sheet mySheet;
        public HitboxManager boxManager;
        public bool dbg;
        //public variables
        [HideInInspector]
        public bool initialised;
        public int frame; //the displayed frame
        public float frameRate = 10; //framerate
        public bool isPlaying = true; //if the sheet reader has a sheet to play
        public bool paused = false; //if the reader has paused 
        public bool looping = false; //are we supposed to be looping?
        public bool ignoreTimeScale; //should we play using Time.unscaledTime or just Time.time 
        public bool randomFrameStart; //should we start animating on a random frame?
        public bool pixelPerfect; //should we snap to the exact pixel?

        //local variables
        int startFrame; //the frame we start playing on
        int durationInFrames; //the number of frames between the frame we're starting on and the frame we're ending on
        int playedFrames; //the number of frames played since we started this animation
        [HideInInspector]
        public int playingFrame; //the current animation frame
        Sheet nextSheet; //second sheet used in transitions
        float nextFrameRate = 10; //frame rate for second sheet
        bool nextLooping = false; //loop status for second sheet

        int loops = 0; //number of times we've looped in this animation.
        float startTime; //time when this animation started

        bool hitStop; //are we in hitstop?
        float timeAtHitStop; //time we started hitstop
        Vector2 hitStopPosition;//position when we got hitstopped
        float hitStopDuration; //total hitstop time to process

        Vector3 oldVelocity;
        float oldGravityScale;

        public float time { get { return isPlaying ? globalTime - startTime : 0; } }
        public float duration { get { return (float)durationInFrames / (float)frameRate; } }
        public float globalTime { get { return Time.unscaledTime; } }

        //used to offset sprite and hitboxes when there is a velocity curve changing their positions;
        public Vector2 spatialOffset;

        public UnityEvent<Retro.Sheet> finished = new UnityEvent<Retro.Sheet>();
        public Life life;
        // Use this for initialization
        void Awake() {
            preferences = (RetroboxPrefs)(Resources.Load("Retrobox Preferences"));
            boxManager = new HitboxManager(this);
            if (spriteRenderer == null) {
                spriteRenderer = GetComponent<SpriteRenderer>(); //if not specified otherwise, look for one
                // Debug.LogWarning("no SpriteRenderer attached to " + gameObject.name + ", looking for attached component...");
            }
            if (rigidBody == null) {
                rigidBody = GetComponent<Rigidbody2D>(); //if not specified otherwise, look for one
                //Debug.LogWarning("no sprite Rigidbody2d attached to " + gameObject.name + ", looking for attached component...");
            }

        }

        private void Start() {
            if (isPlaying && nextSheet == null) {
                if (!initialised) {
                    if (randomFrameStart) {
                        Play(mySheet, (int)Random.Range(0, mySheet.spriteList.Count), mySheet.spriteList.Count - 1, frameRate, looping, true);
                    } else {
                        Play(mySheet, frameRate, looping, true);
                    }
                } else {
                    //TODO: I changed override to TRUE in 27/07/22
                    Play(mySheet, frameRate, looping, true);
                }
            }
            initialised = true;

        }

        public void Play(Sheet sheet) { //reset all instance variables and re-run setup.
            Play(sheet, 0, sheet.spriteList.Count, frameRate, false, false);
        }

        public void Play(Sheet sheet, bool looping_) { //reset all instance variables and re-run setup.
            Play(sheet, 0, sheet.spriteList.Count, frameRate, looping_, false);
        }

        public void Play(Sheet sheet, float rate, bool looping_, bool overRide = false) { //reset all instance variables and re-run setup.
            Play(sheet, 0, sheet.spriteList.Count, rate, looping_, overRide);
        }

        public void Play(Sheet sheet, int startFrame_, int durationInFrames_, float rate, bool looping_, bool overRide) { //play an animation

            if (!IsPlayingSheet(sheet) || overRide) {
                mySheet = sheet;
                startFrame = startFrame_;
                durationInFrames = durationInFrames_;
                frameRate = rate;
                startTime = globalTime; //time we started playing this animation.
                playedFrames = 0;
                looping = looping_;
                loops = 0;
                ClearNext();
                if (boxManager == null) {
                    boxManager = new HitboxManager(this);
                }
                boxManager.Initialise();

                isPlaying = true;
                paused = false;

                //Fast Forward to the start frame by cycling through each of the previous.
                for (int i = 0; i <= this.startFrame; i++) {
                    SetFrame(i);
                }
                CheckCompatability(sheet);
            }
        }

        public void PlayMS(Sheet sheet, float frameDuration, bool loop, bool overRide) {
            Play(sheet, 1f / frameDuration, loop, overRide);
        }

        public void PlayTransition(Sheet sheet, float rate, Sheet sheet2, float rate2, bool looping2) { //play one animation after another.
            PlayTransition(sheet, rate, sheet2, rate2, looping2, false);
        }

        public void PlayTransition(Sheet sheet, float rate, Sheet sheet2, float rate2, bool looping2, bool overRide_) { //play one animation after another.
            if (!IsPlayingSheet(sheet) && !IsPlayingSheet(sheet2) || overRide_) {
                loops = 0;

                startFrame = 0;
                durationInFrames = sheet.spriteList.Count;
                playedFrames = 0;

                mySheet = sheet;
                frameRate = rate;

                looping = false;

                nextSheet = sheet2;
                nextFrameRate = rate2;
                nextLooping = looping2;
                startTime = globalTime; //time we started playing this animation.
                if (boxManager == null) {
                    boxManager = new HitboxManager(this);
                }
                boxManager.Initialise();
                isPlaying = true;
                paused = false;

                SetFrame(startFrame);
                CheckCompatability(sheet);
                CheckCompatability(sheet2);
            }
        }
        void ClearNext() {
            nextSheet = null;
            nextFrameRate = 10;
            nextLooping = false;
        }

        public void Clear() {
            boxManager.ClearFrame();
            isPlaying = false;
            spriteRenderer.sprite = null;
        }

        // Update is called once per frame
        void Update() {

            if (isPlaying) { //"Playback Mode"

                bool updated = false;
                //if the calculated frame is different to the current frame, update it.
                if ((int)((globalTime - startTime) * frameRate) != playedFrames && !paused) {
                    playedFrames = (int)((globalTime - startTime) * frameRate);//update the current frame.
                    updated = true;
                }

                //if we're allowed to be updating
                if (playedFrames < durationInFrames || looping) {
                    if (updated) {
                        SetFrame((startFrame + playedFrames) % (mySheet.spriteList.Count));
                        if (playingFrame == 0) {
                            loops++;
                        }
                    }

                } else if (nextSheet != null) {    //otherwise play the next sheet
                    Play(nextSheet, nextFrameRate, nextLooping);
                } else {
                    isPlaying = false; //otherwise finish up.
                    finished.Invoke(mySheet);
                }

            } else { //"puppeteer mode"
                if (frame != playingFrame && !paused) {
                    SetFrame(frame);
                }
            }

            UpdateHitstop();
            ApplySpatialOffset();
            ApplyPixelPerfect();

        }

        public void SetSpatialOffset(Vector2 offset) {
            spatialOffset = offset;
            if (boxManager != null) {
                boxManager.ApplySpatialOffset(spatialOffset);
            }
        }

        void ApplySpatialOffset() {
            //ONLY DO THIS IF WE HAVE A SEPARATE SPRITERENDERER OBJECT
            if (spriteRenderer != null && spriteRenderer.transform != transform) {
                spriteRenderer.transform.localPosition = spatialOffset;
            }
        }

        void ApplyPixelPerfect() {
            if (pixelPerfect) {
                spriteRenderer.transform.position = PixelPerfect();
            }
        }

        private void FixedUpdate() {
            if (boxManager.curveInstancesByLayer.Count > 0) {
                boxManager.UpdateCurves();
            }
        }

        void SetFrame(int newFrame) { //update the colliders / triggers to the given frame
            newFrame = (newFrame % mySheet.spriteList.Count + mySheet.spriteList.Count) % mySheet.spriteList.Count;
            playingFrame = newFrame;
            frame = playingFrame;
            spatialOffset = Vector2.zero;
            boxManager.SetFrame(playingFrame);
            if (spriteRenderer != null) {
                spriteRenderer.sprite = mySheet.spriteList[playingFrame];
            }
            ApplyProperties();
        }

        public void Stop() {
            isPlaying = false;
        }

        public bool IsPlaying() { //Returns true if a sheet has been loaded and has not ended playback. 
            return isPlaying;
        }

        public bool IsPlayingSheet(Sheet sheet) {//Returns true if the specified sheet has been loaded and has not ended playback.
            return (mySheet == sheet && isPlaying);
        }

        public Sheet GetSheet() {//Returns mySheet.
            return mySheet;
        }

        public SpriteRenderer GetSpriteRenderer() { //returns the sprite renderer associated with this animator
            return spriteRenderer;
        }

        public int GetCurrentFrame() { //returns the current frame being shown
            return (startFrame + playedFrames) % (mySheet.spriteList.Count);
        }

        public int GetPlayedFrames() { //returns the amount of frames which have been cycled since the last Play() call.
            return playedFrames;
        }

        public float GetStartTime() { //returns the amount of frames which have been cycled since the last Play() call.
            return startTime;
        }

        public int GetLoops() { //returns the amount of times this animation has cycled since the last Play() call.
            return loops;
        }

        public void SetFrameRate(float r) { //sets the framerate of the animation
            frameRate = r;
        }


        public void HitStop(float hsDuration) {//calculate hitstop on this gameobject
            if (!paused) {
                hitStopDuration = hsDuration;
                hitStopPosition = spriteRenderer.transform.position;

                if (rigidBody != null) { //if we have a rigidbody, freeze it.
                    oldVelocity = (hitStop) ? oldVelocity : (Vector3)rigidBody.velocity;
                    oldGravityScale = (hitStop) ? oldGravityScale : rigidBody.gravityScale;
                    rigidBody.velocity = new Vector2();
                    rigidBody.gravityScale = 0;
                }

                hitStop = true;
                paused = true;
                timeAtHitStop = globalTime;

            }
        }
        void UpdateHitstop() {
            if (IsInHitStop() && globalTime - hitStopDuration > timeAtHitStop) {
                EndHitStop();
            }
        }

        void EndHitStop() {
            spriteRenderer.transform.position = hitStopPosition;

            if (rigidBody != null) {
                rigidBody.gravityScale = oldGravityScale;
                rigidBody.velocity = oldVelocity;
            }

            startTime += ((globalTime - timeAtHitStop)); //make up for the lost time paused so we resume where we left off.
            if (startTime > globalTime) startTime = globalTime;


            paused = false;
            hitStop = false;
        }

        public bool IsInHitStop() { //returns true if hitstop is currently active
            return hitStop;
        }

        public void Finish() { //end the current loop;
            Play(mySheet, playingFrame, mySheet.spriteList.Count - 1 - playingFrame, frameRate, false, true);
        }

        public void SeekToFrame(int i) { //skips to the selected frame without stopping playback
            startFrame = (startFrame + (i - playingFrame)) % mySheet.spriteList.Count;
        }

        public Vector2 PixelPerfect() {
            float ppu = spriteRenderer.sprite.pixelsPerUnit;
            return new Vector2(Mathf.Round(transform.position.x * ppu) / ppu, Mathf.Round(transform.position.y * ppu) / ppu);
        }

        //TODO: FIX THIS entire thing. apply the frame properties here in the animator, instead of in some random static scipt.
        void ApplyProperties() { //applies any effects, frame properties and hitbox properties which occur on this frame.
                                 //SendMessageUpwards("ApplyFrameProperties", mySheet.propertiesList[GetCurrentFrame()], SendMessageOptions.DontRequireReceiver);
            SendMessageUpwards("ApplyFrameProperties", this, SendMessageOptions.DontRequireReceiver);
        }

        public void CheckCompatability(Sheet sheet) {//Checks for version parity between this Retro.BoxAnimator and the Retro.Sheet being played.
            if (!version.Equals(sheet.GetVersion())) {
                Debug.Log(
                    "Warning. Your Sheet Reader " +
                    transform.name + " and sheet " +
                    sheet + " are different versions." +
                    " To avoid compatability issues, update sheets with the latest version of Retrobox Editor"
                );
            }
        }

        public float MapAnimationClipTime(AnimationClip clip) {
            return Helpers.Map(time, 0, duration, 0, clip.length);
        }
        public float MapAnimationClipTime(AnimationClip clip, float boxTime) {
            return Helpers.Map(boxTime, 0, duration, 0, clip.length);
        }
    }
}
