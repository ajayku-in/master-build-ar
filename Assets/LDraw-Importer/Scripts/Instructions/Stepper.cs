using System;
using System.Collections;
using UnityEngine;

namespace LDraw
{
    [RequireComponent(typeof(SubModel))]
    public class Stepper : MonoBehaviour
    {
        // Constants

        private const float STEP_SPEED = 0.2f;

        // Inspector fields

        [SerializeField]
        [Tooltip("The step to go to when X is pressed.")]
        string goToStep = "2.67";

        // Internal state

        SubModel rootModel;

        bool ready = false;

        // Unity lifecycle methods

        void Awake()
        {
            rootModel = GetComponent<SubModel>();
            rootModel.IsRoot = true;
        }

        void Start()
        {
            rootModel.PrepareSteps("", false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearSteps();
            }

            if (ready && Input.GetKeyDown(KeyCode.F))
            {
                NextStep();
            }

            if (ready && Input.GetKeyDown(KeyCode.R))
            {
                PreviousStep();
            }

            if (ready && Input.GetKeyDown(KeyCode.X))
            {
                GoToStep(goToStep);
            }
        }

        public void ClearSteps()
        {
            rootModel.PrepareSteps("", true);
            ready = true;
        }

        // Public methods

        public Step GetCurrentStep()
        {
            return rootModel.GetCurrentStep();
        }

        public void NextStep()
        {
            rootModel.NextStep();
        }

        public void PreviousStep()
        {
            rootModel.PreviousStep();
        }

        public void GoToStep(string stepNumber)
        {
            StopAllCoroutines();
            StartCoroutine(GoToStepCoroutine(stepNumber));
        }

        // Helper methods

        IEnumerator GoToStepCoroutine(string stepNumber)
        {
            // TODO: Validate that the step number exists
            Step currentStep = GetCurrentStep();
            if (currentStep == null)
            {
                // Handle the first step
                rootModel.NextStep();
                currentStep = GetCurrentStep();
            }

            Version stepNumberVersion = new Version(stepNumber);
            while(stepNumberVersion < currentStep.NumberVersion && rootModel.PreviousStep())
            {
                currentStep = GetCurrentStep();
                yield return new WaitForSeconds(STEP_SPEED);
            }

            while (stepNumberVersion > currentStep.NumberVersion && rootModel.NextStep())
            {
                currentStep = GetCurrentStep();
                yield return new WaitForSeconds(STEP_SPEED);
            }
        }
    }

}
