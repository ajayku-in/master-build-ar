using System;
using System.Collections;
using UnityEngine;

namespace LDraw
{
    [RequireComponent(typeof(SubModel))]
    public class Stepper : MonoBehaviour
    {
        private const float STEP_SPEED = 0.2f;

        public bool ready = false;
        public string goToStep = "2.67";

        private SubModel rootModel;

        void Start()
        {
            rootModel = GetComponent<SubModel>();
            rootModel.PrepareSteps("", false);
        }

        // Update is called once per frame
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

        public IEnumerator GoToStepCoroutine(string stepNumber)
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
            while(stepNumberVersion < currentStep.numberVersion && rootModel.PreviousStep())
            {
                currentStep = GetCurrentStep();
                yield return new WaitForSeconds(STEP_SPEED);
            }

            while (stepNumberVersion > currentStep.numberVersion && rootModel.NextStep())
            {
                currentStep = GetCurrentStep();
                yield return new WaitForSeconds(STEP_SPEED);
            }
        }
    }

}
