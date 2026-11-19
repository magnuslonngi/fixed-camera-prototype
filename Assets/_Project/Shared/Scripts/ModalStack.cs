using System;
using System.Collections.Generic;
using UnityEngine;

namespace FixedCameraPrototype.Shared
{
    // Sole authority for which full-screen modals are open and which one has input. While any is open
    // the game is paused and gameplay input is locked; only the top screen receives menu input.
    public class ModalStack : MonoBehaviour
    {
        [SerializeField] private CharacterControlLock m_controlLock;
        [SerializeField] private GamePause m_gamePause;

        private readonly List<IModalScreen> m_openScreens = new List<IModalScreen>();

        public bool HasOpenScreen => m_openScreens.Count > 0;

        public event Action OnStackChanged;

        public bool IsOpen(IModalScreen screen)
        {
            return m_openScreens.Contains(screen);
        }

        public bool IsTop(IModalScreen screen)
        {
            return HasOpenScreen && m_openScreens[m_openScreens.Count - 1] == screen;
        }

        public void Push(IModalScreen screen)
        {
            if (screen == null || IsOpen(screen))
            {
                return;
            }

            if (HasOpenScreen)
            {
                m_openScreens[m_openScreens.Count - 1].SetInputFocus(false);
            }
            else
            {
                m_controlLock.AddLock();
                m_gamePause.AddPause();
            }

            m_openScreens.Add(screen);
            screen.SetInputFocus(true);
            OnStackChanged?.Invoke();
        }

        public void Pop(IModalScreen screen)
        {
            bool wasTop = IsTop(screen);
            if (!m_openScreens.Remove(screen))
            {
                return;
            }

            screen.SetInputFocus(false);

            if (HasOpenScreen)
            {
                if (wasTop)
                {
                    m_openScreens[m_openScreens.Count - 1].SetInputFocus(true);
                }
            }
            else
            {
                m_controlLock.RemoveLock();
                m_gamePause.RemovePause();
            }

            OnStackChanged?.Invoke();
        }
    }
}
