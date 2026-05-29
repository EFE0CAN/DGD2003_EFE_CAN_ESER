using System.Collections;

using UnityEngine;

using UnityEngine.SceneManagement;



/// <summary>

/// Süre bitince: beyaz-gri vücut dumanı, LosePanel, 5 sn sonra sahne restart.

/// </summary>

public class GameTimerFailHandler : MonoBehaviour

{

    [Header("UI")]

    [SerializeField] private GameObject losePanel;



    [Header("Zamanlama")]

    [SerializeField] private float smokeDuration = 3f;

    [SerializeField] private float restartDelay = 5f;



    private bool _handled;



    private void Start()

    {

        if (losePanel != null)

            losePanel.SetActive(false);

    }



    private void OnEnable()

    {

        GameTimer.OnTimeUp += OnTimeUp;

    }



    private void OnDisable()

    {

        GameTimer.OnTimeUp -= OnTimeUp;

    }



    private void OnTimeUp()

    {

        if (_handled) return;

        _handled = true;
        SaveSystem.RecordLoss();

        StartCoroutine(FailSequence());

    }



    private IEnumerator FailSequence()

    {

        Transform character = FindCharacterTransform();

        if (character != null)

            SmokeEffect.PlayBodySmokeAura(character, smokeDuration);



        if (losePanel != null)

            losePanel.SetActive(true);



        DisablePlayerControl();



        yield return new WaitForSeconds(restartDelay);



        Scene active = SceneManager.GetActiveScene();

        SceneManager.LoadScene(active.buildIndex);

    }



    private static Transform FindCharacterTransform()

    {

        var controller = FindFirstObjectByType<FirstPersonCharacterController>();

        if (controller != null)

            return controller.transform;



        Camera mainCam = Camera.main;

        if (mainCam != null)

            return mainCam.transform;



        GameObject mainCamGo = GameObject.Find("MainCam");

        return mainCamGo != null ? mainCamGo.transform : null;

    }



    private static void DisablePlayerControl()

    {

        var controller = FindFirstObjectByType<FirstPersonCharacterController>();

        if (controller != null)

            controller.enabled = false;



        var interaction = FindFirstObjectByType<PlayerInteraction>();

        if (interaction != null)

            interaction.enabled = false;

    }

}

