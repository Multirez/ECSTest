using UnityEngine;
using System.Collections;

namespace Testing
{
    public class FPS : MonoBehaviour
    {
        private float _fps;
        private float _avarage = 5;
        private float _max = 0;
        private float _min = 100;
        private float _lastFrame;
        private float _localMax;
        private float _localMin;
        private int _localFrameCount = 0;
        private string _showString = "";

        [SerializeField] int _targetFrameRate = 0;

        public float Value => _fps;

        void Start()
        {
            Application.targetFrameRate = _targetFrameRate;
        }

        void Update()
        {
            float thisFrame = Time.realtimeSinceStartup;
            float deltaTime = thisFrame - _lastFrame;
            if ((int)thisFrame > _lastFrame)
                UpdateMax();
            _lastFrame = thisFrame;

            _localFrameCount++;
            if (_localMax < deltaTime)
                _localMax = deltaTime;
            if (_localMin > deltaTime)
                _localMin = deltaTime;
        }

        private void UpdateMax()
        {
            _fps = _localFrameCount;
            _avarage = 1f / _fps;
            _localFrameCount = 0;

            _max = _localMax;
            _localMax = 0;

            _min = _localMin;
            _localMin = float.MaxValue;

            _showString = string.Format("fps:{0} frame:{1}ms min:{2}ms max:{3}ms", _fps.ToString("0.0"),
                (_avarage * 1000).ToString("0.0"), (_min * 1000).ToString("0.0"), (_max * 1000).ToString("0.0"));

            //showString += $" mem:{SystemInfo.systemMemorySize}MB";
        }

        void OnGUI()
        {
            var content = new GUIContent(_showString);
            float minWidth, maxWidth;
            GUI.skin.label.CalcMinMaxWidth(content, out minWidth, out maxWidth);
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(maxWidth + 10));
            GUILayout.Label(_showString);
            GUILayout.EndVertical();
        }
    }
}
