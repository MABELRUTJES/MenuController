using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class MenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    private InputAction _navigate;

    private void Awake()
    {
        AutoWireIfNeeded();

        ConfigureUiInputModule();

        BindNavigateForFirstSelect();

        if (playButton != null) playButton.onClick.AddListener(() => Debug.Log("Play clicked"));
        if (optionsButton != null) optionsButton.onClick.AddListener(() => Debug.Log("Options clicked"));
        if (quitButton != null) quitButton.onClick.AddListener(() => Debug.Log("Quit clicked"));
    }

    private void OnEnable()
    {
        if (_navigate != null) _navigate.performed += OnNavigatePerformed;
    }

    private void OnDisable()
    {
        if (_navigate != null) _navigate.performed -= OnNavigatePerformed;
    }

    private void OnDestroy()
    {
        if (_navigate != null)
        {
            _navigate.Disable();
            _navigate.Dispose();
            _navigate = null;
        }
    }

    private static void ConfigureUiInputModule()
    {
        // If only actionsAsset is assigned in the scene, the per-action references can be empty.
        // Explicitly wire them so controller + keyboard + mouse all work.
        var uiModule = FindFirstObjectByType<InputSystemUIInputModule>();
        if (uiModule == null) return;

        var asset = uiModule.actionsAsset;
        if (asset == null) return;

        var map = asset.FindActionMap("UI", throwIfNotFound: false);
        if (map == null) return;

        InputActionReference RefFor(string actionName)
        {
            var action = map.FindAction(actionName, throwIfNotFound: false);
            return action != null ? InputActionReference.Create(action) : null;
        }

        if (uiModule.move == null) uiModule.move = RefFor("Navigate");
        if (uiModule.submit == null) uiModule.submit = RefFor("Submit");
        if (uiModule.cancel == null) uiModule.cancel = RefFor("Cancel");
        if (uiModule.point == null) uiModule.point = RefFor("Point");
        if (uiModule.leftClick == null) uiModule.leftClick = RefFor("Click");

        uiModule.deselectOnBackgroundClick = false;
    }

    private void BindNavigateForFirstSelect()
    {
        // Don't preselect for mouse users.
        // When the user starts navigating with controller/keyboard, select the first button.
        var uiModule = FindFirstObjectByType<InputSystemUIInputModule>();
        if (uiModule == null || uiModule.actionsAsset == null) return;

        var map = uiModule.actionsAsset.FindActionMap("UI", throwIfNotFound: false);
        if (map == null) return;

        var action = map.FindAction("Navigate", throwIfNotFound: false);
        if (action == null) return;

        _navigate = action.Clone();
        _navigate.Enable();
    }

    private void OnNavigatePerformed(InputAction.CallbackContext _)
    {
        if (EventSystem.current == null) return;
        if (EventSystem.current.currentSelectedGameObject != null) return;
        if (playButton == null) return;

        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    private void AutoWireIfNeeded()
    {
        if (playButton == null) playButton = FindButton("PlayButton");
        if (optionsButton == null) optionsButton = FindButton("OptionsButton");
        if (quitButton == null) quitButton = FindButton("QuitButton");
    }

    private static Button FindButton(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Button>() : null;
    }
}
