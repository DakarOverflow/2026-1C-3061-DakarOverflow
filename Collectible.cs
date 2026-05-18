using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    // Definimos los tres tipos de coleccionables exigidos por la cátedra
    public enum CollectibleType
    {
        FuelTank, // Tanque de combustible
        Wrench,   // Llave inglesa
        Coin      // Moneda
    }

    public class Collectible
    {   // Atributos
        public CollectibleType Type { get; private set; }
        
        // True por defecto, cambia a false cuando el jugador lo recoge. Esto define si se muestra en pantalla.
        public bool IsActive { get; private set; }
    
        public Vector3 Position { get; private set; }
        
        // Volumen para la Detección de Colisiones, el enunciado permite usar BoudingBox para los coleccionables.
        public BoundingBox Collider { get; private set; }

        private CustomModel _model;
        private Matrix _world;

        // Constructor
        public Collectible(CollectibleType type, CustomModel model, Vector3 initialPosition)
        {
            Type = type;
            _model = model;
            Position = initialPosition;
            IsActive = true;
            
            // Matriz de mundo inicial
            _world = Matrix.CreateTranslation(Position);
            
            // Creamos un BoundingBox inicial alrededor de la posición del coleccionable.
            // TODO: Ajustar el tamaño del BoundingBox al tamaño real de los modelos 3D.
            Collider = new BoundingBox(
                Position - new Vector3(10f, 10f, 10f), 
                Position + new Vector3(10f, 10f, 10f)
            );
        }

        // Update actualizar el estado del coleccionable
        public void Update(GameTime gameTime)
        {
            // Si ya fue recogido, no hacemos nada
            if (!IsActive) return;

            // --- PREPARACIÓN PARA LA 4TA ENTREGA ---
            // TODO: Los coleccionables deben tener rotación en Y y rebote vertical.
            // TODO: Como el modelo va a estar rebotando y rotando, hay que actualizar la posición del BoundingBox también.
        }

        // Draw dibuja el coleccionable si está activo
        public void Draw(Matrix view, Matrix projection)
        {
            if (!IsActive) return;
            _model.Draw(_world, view, projection);
        }

        // Método que se llamará al detectar una colisión entre el auto y el coleccionable
        // TODO: Definir si lo llama TGCGame o TileManager
        public void PickUp(Vehicle player)
        {
            if (!IsActive) return;

            IsActive = false; // Se desactiva el coleccionable para que desaparezca de la pista

            // Se aplica el efecto correspondiente a los medidores del jugador
            switch (Type)
            {
                case CollectibleType.FuelTank:
                    // TODO: Restaurar combustible en el auto.
                    // Ej: player.AddFuel(20f); 
                    break;
                
                case CollectibleType.Wrench:
                    // TODO: Restaurar puntos de daño/salud en el auto.
                    // Ej: player.RepairDamage(15f);
                    break;
                
                case CollectibleType.Coin:
                    // TODO: Incrementar el puntaje del jugador.
                    // Ej: player.AddScore(100);
                    break;
            }
        }
    }
}