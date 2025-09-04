namespace ActionSimilarity;

[CreateAssetMenu(menuName = "Trials/Trial Settings")]
public class TrialSettings : ScriptableObject
{
    [Header("Shapes")]
    public ShapeSettings shapeSettings;
	
	[Header("Fixation")]
	public FixationSettings fixationSettings;

    [Header("Turn")]
    public float turnTime = 2.5f;
    public float halfwayFlashMs = 11f;
}