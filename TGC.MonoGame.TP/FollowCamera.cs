using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Zero;

/// <summary>
///     Una camara que sigue objetos.
/// </summary>
internal class FollowCamera : Camera
{
    private const float AxisDistanceToTarget = 1000f;
    private const float AngleFollowSpeed = 0.015f;
    private const float AngleThreshold = 0.85f;

    private Vector3 _currentForwardVector = Vector3.Forward;
    private Vector3 _pastForwardVector = Vector3.Forward;
    private float _forwardVectorInterpolator;
    private readonly GraphicsDevice _graphicsDevice;

    /// <summary>
    ///     Crea una FollowCamera que sigue a una matriz de mundo.
    /// </summary>
    /// <param name="aspectRatio"></param>
    public FollowCamera(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            graphicsDevice.Viewport.AspectRatio,
            0.1f,
            4000f
        );
    }

    private Matrix _projection;

    private Matrix _view;

    public void OnClientSizeChanged(object sender, EventArgs e)
    {
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4, _graphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
    }

    public static string GetName()
    {
        return "Follow Camera";
    }

    public Matrix GetView()
    {
        return _view;
    }

    public Matrix GetProjection()
    {
        return _projection;
    }


    /// <summary>
    ///     Actualiza la Camara usando una matriz de mundo actualizada para seguirla.
    /// </summary>
    /// <param name="gameTime">The Game Time to calculate framerate-independent movement</param>
    /// <param name="followedWorld">The World matrix to follow</param>
    public void Update(GameTime gameTime, Matrix followedWorld)
    {
        // Obtengo el tiempo.
        var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

        // Obtengo la posicion de la matriz de mundo que estoy siguiendo.
        var followedPosition = followedWorld.Translation;

        // Obtengo el vector Adelante de la matriz de mundo que estoy siguiendo.
        var followedForward = followedWorld.Forward;

        // Si el producto escalar entre el vector Adelante anterior
        // y el actual es mas grande que un limite,
        // muevo el Interpolator (desde 0 a 1) mas cerca de 1.
        if (Vector3.Dot(followedForward, _pastForwardVector) > AngleThreshold)
        {
            // Incremento el Interpolator.
            _forwardVectorInterpolator += elapsedTime * AngleFollowSpeed;

            // No permito que Interpolator pase de 1.
            _forwardVectorInterpolator = MathF.Min(_forwardVectorInterpolator, 1f);

            // Calculo el vector Adelante a partir de la interpolacion.
            // Esto mueve el vector Adelante para igualar al vector Adelante que sigo.
            // En este caso uso la curva x^2 para hacerlo mas suave.
            // Interpolator se convertira en 1 eventualmente.
            _currentForwardVector = Vector3.Lerp(_currentForwardVector, followedForward,
                _forwardVectorInterpolator * _forwardVectorInterpolator);
        }
        else
            // Si el angulo no pasa de cierto limite, lo pongo de nuevo en cero.
        {
            _forwardVectorInterpolator = 0f;
        }

        // Guardo el vector Adelante para usar en la siguiente iteracion.
        _pastForwardVector = followedForward;

        // Calculo la posicion de la camara
        // tomo la posicion que estoy siguiendo, agrego un offset hacia atras y hacia arriba.
        // Restar el vector Adelante (-Forward = Backward) coloca la camara detras del auto.
        var offsetedPosition = followedPosition
                               - _currentForwardVector * AxisDistanceToTarget
                               + Vector3.Up * (AxisDistanceToTarget * 0.4f); // Altura de la camara ajustada

        // Calculo el vector Arriba actualizado.
        // Nota: No se puede usar el vector Arriba por defecto (0, 1, 0).
        // Como no es correcto, se calcula con este truco de producto vectorial.

        // Calcular el vector Adelante haciendo la resta entre el destino y el origen
        // y luego normalizandolo (Esta operacion es cara!).
        // (La siguiente operacion necesita vectores normalizados)
        var forward = followedPosition - offsetedPosition;
        forward.Normalize();

        // Obtengo el vector Derecha asumiendo que la camara tiene el vector Arriba apuntando hacia arriba
        // y no esta rotada en el eje X (Roll).
        var right = Vector3.Cross(forward, Vector3.Up);

        // Una vez que tengo la correcta direccion Derecha, obtengo la correcta direccion Arriba usando
        // otro producto vectorial.
        var cameraCorrectUp = Vector3.Cross(right, forward);

        // Calculo la matriz de Vista de la camara usando la Posicion, La Posicion a donde esta mirando,
        // y su vector Arriba.
        _view = Matrix.CreateLookAt(offsetedPosition, followedPosition, cameraCorrectUp);
    }
}