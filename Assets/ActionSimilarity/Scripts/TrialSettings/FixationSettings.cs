namespace ActionSimilarity;

[Serializable]
public class FixationSettings
{
        public GameObject fixationSphere;
        [SerializeField]
        private int fixationDepth;
        public int FixationDepth => fixationDepth;

        public TextMeshPro textMeshPro;

        public float turnTime;
    }
