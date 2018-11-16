using UnityEngine;

namespace Qlay.Functions
{
    class Warhead
    {

        public void Enable()
        {
            AlphaWarheadNukesitePanel[] WarheadNukePanel = Object.FindObjectsOfType<AlphaWarheadNukesitePanel>();
            for (int i = 0; i < WarheadNukePanel.Length; i++)
            {
                AlphaWarheadNukesitePanel alphaWarheadNukesitePanel = WarheadNukePanel[i];
                if (alphaWarheadNukesitePanel != null)
                {
                    alphaWarheadNukesitePanel.SetEnabled(true);
                }
            }
        }

        public void Disable()
        {
            AlphaWarheadNukesitePanel[] WarheadNukePanel = Object.FindObjectsOfType<AlphaWarheadNukesitePanel>();
            for (int i = 0; i < WarheadNukePanel.Length; i++)
            {
                AlphaWarheadNukesitePanel alphaWarheadNukesitePanel = WarheadNukePanel[i];
                if (alphaWarheadNukesitePanel != null)
                {
                    alphaWarheadNukesitePanel.SetEnabled(false);
                }
            }
        }

        public void Start()
        {
            AlphaWarheadController[] WarheadController = Object.FindObjectsOfType<AlphaWarheadController>();
            for (int j = 0; j < WarheadController.Length; j++)
            {
                AlphaWarheadController alphaWarheadController = WarheadController[j];
                if (alphaWarheadController != null)
                {
                    alphaWarheadController.StartDetonation();
                }
            }
        }

        public void Stop()
        {
            AlphaWarheadController[] WarheadController = Object.FindObjectsOfType<AlphaWarheadController>();
            for (int j = 0; j < WarheadController.Length; j++)
            {
                AlphaWarheadController alphaWarheadController = WarheadController[j];
                if (alphaWarheadController != null)
                {
                    alphaWarheadController.CancelDetonation();
                }
            }
        }

        public void Lock()
        {
            AlphaWarheadController[] WarheadController = Object.FindObjectsOfType<AlphaWarheadController>();
            for (int k = 0; k < WarheadController.Length; k++)
            {
                AlphaWarheadController alphaWarheadController = WarheadController[k];
                if (alphaWarheadController != null)
                {
                    alphaWarheadController.SetLocked(true);
                }
            }
        }

        public void UnLock()
        {
            AlphaWarheadController[] WarheadController = Object.FindObjectsOfType<AlphaWarheadController>();
            for (int k = 0; k < WarheadController.Length; k++)
            {
                AlphaWarheadController alphaWarheadController = WarheadController[k];
                if (alphaWarheadController != null)
                {
                    alphaWarheadController.SetLocked(false);
                }
            }
        }
    }
}
