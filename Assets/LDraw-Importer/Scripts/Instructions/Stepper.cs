using System;
using System.Collections;
using UnityEngine;

namespace LDraw
{
    [RequireComponent(typeof(SubModel))]
    public class Stepper : MonoBehaviour
    {
        public bool ready = false;
        public string goToStep = "2.67";

        private SubModel rootModel;

        void Start()
        {
            rootModel = GetComponent<SubModel>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                rootModel.ClearSteps("");
                ready = true;
            }

            if (ready && Input.GetKeyDown(KeyCode.F))
            {
                rootModel.NextStep();
            }

            if (ready && Input.GetKeyDown(KeyCode.R))
            {
                rootModel.PreviousStep();
            }

            if (ready && Input.GetKeyDown(KeyCode.X))
            {
                StopAllCoroutines();
                StartCoroutine(GoToStep(goToStep));
            }
        }

        public Step GetCurrentStep()
        {
            return rootModel.GetCurrentStep();
        }

        public IEnumerator GoToStep(string stepNumber)
        {
            // TODO: Validate that the step number exists
            Step currentStep = GetCurrentStep();
            Version stepNumberVersion = new Version(stepNumber);
            while(stepNumberVersion < currentStep.numberVersion && rootModel.PreviousStep())
            {
                currentStep = GetCurrentStep();
                yield return new WaitForSeconds(0.05f);
            }

            while (stepNumberVersion > currentStep.numberVersion && rootModel.NextStep())
            {
                currentStep = GetCurrentStep();
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

}
