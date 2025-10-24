using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Shooter.Managers
{
    public class SoundManager
    {
        private SoundEffect _shootSound;
        private SoundEffect _explosionSound;
        private SoundEffect _hitSound;
        private SoundEffect _enemyShootSound;

        public void LoadContent(ContentManager content)
        {
            _shootSound = content.Load<SoundEffect>("shoot");
            _explosionSound = content.Load<SoundEffect>("explosion");
            _hitSound = content.Load<SoundEffect>("hit");
            _enemyShootSound = content.Load<SoundEffect>("enemy_shoot");
        }

        public void PlayShoot()
        {
            _shootSound?.Play();
        }

        public void PlayExplosion()
        {
            _explosionSound?.Play();
        }

        public void PlayHit()
        {
            _hitSound?.Play();
        }

        public void PlayEnemyShoot()
        {
            _enemyShootSound?.Play();
        }
    }
}

