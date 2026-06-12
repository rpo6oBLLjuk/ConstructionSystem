using System;
using UnityEngine;
using Zenject;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionManager : MonoBehaviour
    {
        [Inject] ActiveProjectService _activeProjectService;

        public event Action<ProjectData> ProjectLoaded;


        private void Start()
        {
#if UNITY_EDITOR
            if (_activeProjectService.ProjectData == null)
            {
                this.FastLog("<b><u> ––– START PLAY FROM LOGIN SCENE TO LOAD PROJECT BLUEPRINT</u></b> –––");
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
#endif

            ProjectLoaded?.Invoke(_activeProjectService.ProjectData);
        }
    }
}
