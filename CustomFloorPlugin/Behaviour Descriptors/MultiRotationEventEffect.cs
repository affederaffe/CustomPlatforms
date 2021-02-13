using UnityEngine;


namespace CustomFloorPlugin
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Unity can't deserialize data onto readonly fields")]
    public class MultiRotationEventEffect : MonoBehaviour
    {
        internal Actor Create()
        {
            Actor actor = gameObject.AddComponent<Actor>();
            actor.Init(_transformL, _transformR, _toggleGameObjects, _rotationVectorsL, _rotationVectorsR, _eventL, _eventR, _useRandomValues);
            return actor;
        }
        internal class Actor : MonoBehaviour
        {
            //private Transform[] _transformL;
            //private Transform[] _transformR;
            private RotationData _rotationDataL;
            private RotationData _rotationDataR;
            private bool _toggleGameObjects;
            private Vector3[] _rotationVectorsL;
            private Vector3[] _rotationVectorsR;
            private SongEventType _eventL;
            private SongEventType _eventR;
            private bool _useRandomValues;

            private int _randomGenerationFrameNum = -1;
            private float _randomDirection;
            private float _randomStartRotation;

            internal void Init(Transform[] _transformL, Transform[] _transformR, bool _toggleGameObjects, Vector3[] _rotationVectorsL, Vector3[] _rotationVectorsR, SongEventType _eventL, SongEventType _eventR, bool _useRandomValues)
            {
                //this._transformL = _transformL;
                //this._transformR = _transformR;
                this._toggleGameObjects = _toggleGameObjects;
                this._rotationVectorsL = _rotationVectorsL;
                this._rotationVectorsR = _rotationVectorsR;
                this._eventL = _eventL;
                this._eventR = _eventR;
                this._useRandomValues = _useRandomValues;
                Quaternion[] rotL = new Quaternion[_transformL.Length];
                Quaternion[] rotR = new Quaternion[_transformR.Length];
                for (int i = 0; i < _transformL.Length; i++)
                {
                    rotL[i] = _transformL[i].rotation;
                }
                for (int i = 0; i < _transformR.Length; i++)
                {
                    rotR[i] = _transformR[i].rotation;
                }

                _rotationDataL = new RotationData
                {
                    enabled = false,
                    rotationSpeed = 0f,
                    startRotations = rotL,
                    transforms = _transformL,
                    rotationVector = _rotationVectorsL
                };
                _rotationDataR = new RotationData
                {
                    enabled = false,
                    rotationSpeed = 0f,
                    startRotations = rotR,
                    transforms = _transformR,
                    rotationVector = _rotationVectorsR
                };
                enabled = false;
                if (_toggleGameObjects)
                {
                    for (int i = 0; i < _rotationVectorsL.Length; i++)
                    {
                        _rotationDataL.transforms[i].gameObject.SetActive(false);
                    }
                    for (int i = 0; i < _rotationVectorsR.Length; i++)
                    {
                        _rotationDataR.transforms[i].gameObject.SetActive(false);
                    }
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
            private void Update()
            {
                if (_rotationDataL.enabled)
                {
                    for (int i = 0; i < _rotationVectorsL.Length; i++)
                    {
                        _rotationDataL.transforms[i].Rotate(_rotationVectorsL[i], Time.deltaTime * _rotationDataL.rotationSpeed, Space.Self);
                    }
                }
                if (_rotationDataR.enabled)
                {
                    for (int i = 0; i < _rotationVectorsR.Length; i++)
                    {
                        _rotationDataR.transforms[i].Rotate(_rotationVectorsR[i], Time.deltaTime * _rotationDataR.rotationSpeed, Space.Self);
                    }
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
            private void Destroy()
            {
                if (_toggleGameObjects)
                {
                    UpdateRotationData(0, ref _rotationDataL, _randomStartRotation, _randomDirection);
                    UpdateRotationData(0, ref _rotationDataR, _randomStartRotation, _randomDirection);
                    for (int i = 0; i < _rotationVectorsL.Length; i++)
                    {
                        _rotationDataL.transforms[i].gameObject.SetActive(true);
                    }
                    for (int i = 0; i < _rotationVectorsR.Length; i++)
                    {
                        _rotationDataR.transforms[i].gameObject.SetActive(true);
                    }
                }
            }

            internal void EventCallback(BeatmapEventData beatmapEventData)
            {

                if (beatmapEventData.type == (BeatmapEventType)_eventL || beatmapEventData.type == (BeatmapEventType)_eventR)
                {
                    int frameCount = Time.frameCount;
                    if (_randomGenerationFrameNum != frameCount)
                    {
                        if (!_useRandomValues)
                        {
                            _randomDirection = ((beatmapEventData.type == (BeatmapEventType)_eventL) ? 1f : -1f);
                            _randomStartRotation = ((beatmapEventData.type == (BeatmapEventType)_eventL) ? frameCount : (-frameCount));
                        }
                        else
                        {
                            _randomDirection = (Random.value > 0.5f) ? 1f : -1f;
                            _randomStartRotation = Random.Range(0f, 360f);
                        }
                        _randomGenerationFrameNum = Time.frameCount;
                    }
                    if (beatmapEventData.type == (BeatmapEventType)_eventL)
                    {
                        UpdateRotationData(beatmapEventData.value, ref _rotationDataL, _randomStartRotation, _randomDirection);
                    }
                    if (beatmapEventData.type == (BeatmapEventType)_eventR)
                    {
                        UpdateRotationData(beatmapEventData.value, ref _rotationDataR, -_randomStartRotation, -_randomDirection);
                    }
                    enabled = (_rotationDataL.enabled || _rotationDataR.enabled);
                }
            }

            private void UpdateRotationData(int beatmapEventDataValue, ref RotationData rotationData, float startRotationOffset, float direction)
            {
                if (beatmapEventDataValue == 0)
                {
                    rotationData.enabled = false;
                    for (int i = 0; i < rotationData.transforms.Length; i++)
                    {
                        rotationData.transforms[i].localRotation = rotationData.startRotations[i];
                        if (_toggleGameObjects)
                        {
                            rotationData.transforms[i].gameObject.SetActive(false);
                        }
                    }
                    return;
                }
                if (beatmapEventDataValue > 0)
                {
                    rotationData.enabled = true;
                    for (int i = 0; i < rotationData.transforms.Length; i++)
                    {
                        if (_toggleGameObjects)
                        {
                            rotationData.transforms[i].gameObject.SetActive(true);
                        }
                        rotationData.transforms[i].localRotation = rotationData.startRotations[i];
                        rotationData.transforms[i].Rotate(rotationData.rotationVector[i], startRotationOffset, Space.Self);
                    }
                    rotationData.rotationSpeed = beatmapEventDataValue * 20f * direction;
                }
            }
        }

#pragma warning disable CS0649 //Private, unassigned, instance fields. SuppressMessage doesn't work on this particualr warning...
        [SerializeField]
        private Transform[] _transformL;

        [SerializeField]
        private Transform[] _transformR;

        [SerializeField]
        [Tooltip("There needs to be one Rotation vector per transform, they are local vectors, they are normalized afterwards")]
        private Vector3[] _rotationVectorsL;

        [SerializeField]
        [Tooltip("There needs to be one Rotation vector per transform, they are local vectors")]
        private Vector3[] _rotationVectorsR;
        [Space]
        [SerializeField]
        [Range(0, 15)]
        private SongEventType _eventL;

        [SerializeField]
        [Range(0, 15)]
        private SongEventType _eventR;
        [Space]
        [SerializeField]
        private bool _useRandomValues;

        [SerializeField]
        private bool _toggleGameObjects;
#pragma warning restore CS0649
        internal struct RotationData
        {
            internal bool enabled;
            internal float rotationSpeed;
            internal Quaternion[] startRotations;
            internal Transform[] transforms;
            internal Vector3[] rotationVector;
        }
    }
}