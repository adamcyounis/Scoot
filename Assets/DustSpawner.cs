using UnityEngine;

public class DustSpawner : MonoBehaviour {
    public ParticleSystem ps;
    ParticleSystem.MainModule main => ps.main;
    public static DustSpawner spawner;

    float startSpeedMin;
    float startSpeedMax;
    // Start is called before the first frame update
    void Start() {
        transform.SetParent(null);
        transform.position = default;
        startSpeedMin = ps.main.startSpeed.constantMin;
        startSpeedMax = ps.main.startSpeed.constantMax;
    }

    // Update is called once per frame
    void Update() {

    }

    public void BurstParticles(Vector2 position, Vector2 velocity, int volume = 1) {
        volume = Mathf.Max(volume, 1);

        Vector3 pos = new Vector3(position.x, position.y);

        // ps.transform.position = new Vector3(position.x, position.y, layerManager.GetZ());
        ParticleSystem.ShapeModule s = ps.shape;

        float angle = Vector2.SignedAngle(velocity.normalized, Vector2.up);
        s.rotation = new Vector3(angle, 90, 0);

        ParticleSystem.MainModule module = ps.main;
        module.startSpeed = new ParticleSystem.MinMaxCurve(startSpeedMin * velocity.magnitude, startSpeedMax * velocity.magnitude);

        var emitParams = new ParticleSystem.EmitParams {
            position = pos,
            applyShapeToPosition = true,

        };
        ps.Emit(emitParams, volume);


    }
}