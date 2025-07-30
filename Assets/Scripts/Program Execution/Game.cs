namespace Program_Execution
{
    internal class Game
    {
        public GameStateMachine StateMachine; //будем обращаться извне

        public Game()
        {
            StateMachine = new GameStateMachine(); //Подключаем стейт машину в конструкторе  
        }
    }
}


// private void Awake()
// {
//     InitializeManagers();
// }
//
// private void InitializeManagers()
// {
//     //throw new System.NotImplementedException();