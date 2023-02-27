using UnityEngine;

namespace Nodemon
{
    public class UniversalGUIRenderer : MonoBehaviour
    {
        private void OnGUI()
        {
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            UniversalGUIPopup.HandleMouseBlocking(rect);
            GUILayout.BeginArea(rect);

            OnGUIInternal();            
            
            GUILayout.EndArea();
            UniversalGUIPopup.OnGUI(rect);
        }

        protected virtual void OnGUIInternal()
        {
            
        }
    }
}