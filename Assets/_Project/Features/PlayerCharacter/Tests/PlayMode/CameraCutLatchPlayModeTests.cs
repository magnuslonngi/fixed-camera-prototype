using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using FixedCameraPrototype.Shared;

namespace FixedCameraPrototype.PlayerCharacter.Tests
{
    public class CameraCutLatchPlayModeTests : InputTestFixture
    {
        private const float k_directionTolerance = 0.01f;

        private Gamepad m_gamepad;
        private Transform m_cameraTransform;
        private CameraCutEventChannel m_cameraCutChannel;
        private CameraRelativeInputBasis m_inputBasis;
        private GameObject m_playerObject;
        private InputActionAsset m_inputActionAsset;

        public override void Setup()
        {
            base.Setup();
            m_gamepad = InputSystem.AddDevice<Gamepad>();

            m_cameraTransform = new GameObject("TestCamera").transform;
            m_cameraCutChannel = ScriptableObject.CreateInstance<CameraCutEventChannel>();

            m_inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap map = m_inputActionAsset.AddActionMap("Player");
            InputAction move = map.AddAction("Move", InputActionType.Value, "<Gamepad>/leftStick");
            InputAction sprint = map.AddAction("Sprint", InputActionType.Button, "<Gamepad>/buttonSouth");

            // Inactive while wiring: PlayerInputReader enables its actions in OnEnable.
            m_playerObject = new GameObject("TestPlayer");
            m_playerObject.SetActive(false);
            PlayerInputReader reader = m_playerObject.AddComponent<PlayerInputReader>();
            m_inputBasis = m_playerObject.AddComponent<CameraRelativeInputBasis>();
            SetPrivateField(reader, "m_moveAction", InputActionReference.Create(move));
            SetPrivateField(reader, "m_sprintAction", InputActionReference.Create(sprint));
            SetPrivateField(m_inputBasis, "m_inputReader", reader);
            SetPrivateField(m_inputBasis, "m_viewCameraTransform", m_cameraTransform);
            SetPrivateField(m_inputBasis, "m_cameraCutChannel", m_cameraCutChannel);
            m_playerObject.SetActive(true);
        }

        public override void TearDown()
        {
            Object.Destroy(m_playerObject);
            Object.Destroy(m_cameraTransform.gameObject);
            Object.Destroy(m_cameraCutChannel);
            Object.Destroy(m_inputActionAsset);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator CameraCut_WhileStickHeld_KeepsPreCutWorldDirection()
        {
            Set(m_gamepad.leftStick, Vector2.up);
            yield return null;
            Vector3 directionBeforeCut = m_inputBasis.ToWorldDirection(Vector2.up);

            RotateCameraAndRaiseCut(180.0f);
            yield return null;

            Assert.IsTrue(m_inputBasis.IsLatched);
            AssertDirectionsMatch(directionBeforeCut, m_inputBasis.ToWorldDirection(Vector2.up));
        }

        [UnityTest]
        public IEnumerator CameraCut_AfterStickReleased_UsesNewShotBasis()
        {
            Set(m_gamepad.leftStick, Vector2.up);
            yield return null;
            RotateCameraAndRaiseCut(180.0f);
            yield return null;

            Set(m_gamepad.leftStick, Vector2.zero);
            yield return null;
            Set(m_gamepad.leftStick, Vector2.up);
            yield return null;

            Assert.IsFalse(m_inputBasis.IsLatched);
            AssertDirectionsMatch(Vector3.back, m_inputBasis.ToWorldDirection(Vector2.up));
        }

        [UnityTest]
        public IEnumerator CameraCut_WithStickReleased_DoesNotLatch()
        {
            yield return null;

            RotateCameraAndRaiseCut(180.0f);
            yield return null;

            Assert.IsFalse(m_inputBasis.IsLatched);
        }

        private void RotateCameraAndRaiseCut(float yawDegrees)
        {
            m_cameraTransform.rotation = Quaternion.Euler(0.0f, yawDegrees, 0.0f);
            m_cameraCutChannel.RaiseCameraCut();
        }

        private static void AssertDirectionsMatch(Vector3 expected, Vector3 actual)
        {
            Assert.Less(Vector3.Distance(expected, actual), k_directionTolerance, $"expected {expected}, got {actual}");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }
    }
}
