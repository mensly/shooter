using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Shooter.Managers
{
    public class InputManager
    {
        private KeyboardState _previousKeyboardState;
        private GamePadState _previousGamePadState;
        
        // Input detection for adaptive UI
        private bool _usingKeyboard;
        private bool _usingGamepad;
        private float _keyboardInputTime;
        private float _gamepadInputTime;
        
        public bool UsingKeyboard => _usingKeyboard;
        public bool UsingGamepad => _usingGamepad;
        
        public InputManager()
        {
            _previousKeyboardState = Keyboard.GetState();
            _previousGamePadState = GamePad.GetState(PlayerIndex.One);
            
            _usingKeyboard = false;
            _usingGamepad = false;
            _keyboardInputTime = 0f;
            _gamepadInputTime = 0f;
        }
        
        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(PlayerIndex.One);
            
            TrackInputUsage(keyboardState, gamePadState, gameTime);
        }
        
        public void EndFrame()
        {
            // Update previous states at the end of the frame
            _previousKeyboardState = Keyboard.GetState();
            _previousGamePadState = GamePad.GetState(PlayerIndex.One);
        }
        
        public bool IsShooting(KeyboardState keyboardState, GamePadState gamePadState)
        {
            // Only shoot when button transitions from not pressed to pressed
            bool spacePressed = keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space);
            bool buttonAPressed = gamePadState.IsButtonDown(Buttons.A) && !_previousGamePadState.IsButtonDown(Buttons.A);
            
            // Trigger buttons (check if pressed beyond threshold)
            bool leftTriggerPressed = gamePadState.Triggers.Left > 0.5f && _previousGamePadState.Triggers.Left <= 0.5f;
            bool rightTriggerPressed = gamePadState.Triggers.Right > 0.5f && _previousGamePadState.Triggers.Right <= 0.5f;
            
            return spacePressed || buttonAPressed || leftTriggerPressed || rightTriggerPressed;
        }
        
        public bool IsRestartPressed(KeyboardState keyboardState, GamePadState gamePadState)
        {
            return keyboardState.IsKeyDown(Keys.R) || 
                   (gamePadState.IsButtonDown(Buttons.Start) && !_previousGamePadState.IsButtonDown(Buttons.Start));
        }
        
        public bool IsFullscreenTogglePressed(KeyboardState keyboardState)
        {
            return keyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11);
        }
        
        public bool IsExitPressed(KeyboardState keyboardState, GamePadState gamePadState)
        {
            return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                   keyboardState.IsKeyDown(Keys.Escape);
        }
        
        private void TrackInputUsage(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Check for keyboard input (movement or shooting)
            bool keyboardInput = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.Right) || 
                                keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.D) ||
                                keyboardState.IsKeyDown(Keys.Space);
            
            // Check for gamepad input (movement or shooting)
            bool gamepadInput = gamePadState.ThumbSticks.Left.X != 0 || gamePadState.ThumbSticks.Left.Y != 0 ||
                               gamePadState.DPad.Left == ButtonState.Pressed || gamePadState.DPad.Right == ButtonState.Pressed ||
                               gamePadState.IsButtonDown(Buttons.A) || gamePadState.Triggers.Left > 0.1f || 
                               gamePadState.Triggers.Right > 0.1f;
            
            // Track input time
            if (keyboardInput)
            {
                _keyboardInputTime += deltaTime;
                _usingKeyboard = true;
            }
            
            if (gamepadInput)
            {
                _gamepadInputTime += deltaTime;
                _usingGamepad = true;
            }
        }
        
        public void Reset()
        {
            _usingKeyboard = false;
            _usingGamepad = false;
            _keyboardInputTime = 0f;
            _gamepadInputTime = 0f;
        }
    }
}
