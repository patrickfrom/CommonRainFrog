using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CommonRainFrog.Renderer;

    public class Camera(Vector3 position, float aspectRatio, KeyboardState keyboardState, MouseState mouseState)
    {
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        private float _pitch;

        private float _yaw = -MathHelper.PiOver2;

        private float _fov = MathHelper.PiOver2;
        
        private bool _firstMove = true;
        
        private Vector2 _lastPos;

        public Vector3 Position { get; set; } = position;

        public float AspectRatio { private get; set; } = aspectRatio;

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        private void UpdateVectors()
        {
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }

        public void Update(double time)
        {
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (keyboardState.IsKeyDown(Keys.W))
            {
                Position += Front * cameraSpeed * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                Position -= Front * cameraSpeed * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                Position -= Right * cameraSpeed * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                Position += Right * cameraSpeed * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                Position += Up * cameraSpeed * (float)time;
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                Position -= Up * cameraSpeed * (float)time;
            }

            if (_firstMove)
            {
                _lastPos = new Vector2(mouseState.X, mouseState.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouseState.X - _lastPos.X;
                var deltaY = mouseState.Y - _lastPos.Y;
                _lastPos = new Vector2(mouseState.X, mouseState.Y);

                Yaw += deltaX * sensitivity;
                Pitch -= deltaY * sensitivity;
            }
        }
    }