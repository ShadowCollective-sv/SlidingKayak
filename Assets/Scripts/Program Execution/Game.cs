namespace Program_Execution
{
    internal class Game
    {
        public GameStateMachine StateMachine; //будем обращаться извне
        public Game(ICoroutineRunner coroutineRunner)

        {
            StateMachine = new GameStateMachine(new SceneLoader(coroutineRunner)); //Подключаем стейт машину в конструкторе  
        }
    }
}