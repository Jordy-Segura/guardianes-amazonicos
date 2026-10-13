using System;

namespace GuardianesAmazonicos.Nivel1.HU03
{
    public enum ActivityStage { Introduction, Answers, Correct, Finished }
    public enum AnimalChoice { Mono, Guacamayo, Tortuga }

    // Pure activity state: neither audio nor time limits gate an accessible response.
    public sealed class GuacamayoActivity
    {
        public ActivityStage Stage { get; private set; } = ActivityStage.Introduction;
        public bool CanContinue => Stage == ActivityStage.Correct;
        public event Action Completed;
        public event Action ContinueRequested;

        public void ShowAnswers()
        {
            if (Stage == ActivityStage.Introduction) Stage = ActivityStage.Answers;
        }

        public bool Choose(AnimalChoice choice)
        {
            if (Stage != ActivityStage.Answers) return false;
            if (choice != AnimalChoice.Guacamayo) return false;
            Stage = ActivityStage.Correct;
            Completed?.Invoke();
            return true;
        }

        public void Continue()
        {
            if (!CanContinue) return;
            Stage = ActivityStage.Finished;
            ContinueRequested?.Invoke();
        }
    }
}
