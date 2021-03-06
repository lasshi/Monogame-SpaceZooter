﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceShooter
{
    public class Ufo
    {
        public Vector3 position, flyDir;
        private Game1 game;
        public Texture2D texture;
        public Model model;
        public BoundingBox physicsBody;
        private float angle;
        public float enemySpeed = 0.3f;
        public bool newDir = false;
        public bool shouldDie = false;
        private float lastShot;
        private float shootDelay = 3f;
        public Ufo(Game1 g,Model mod, Texture2D tex)
        {
            game = g;
            model = mod;
            texture = tex;
            position = CreateSpawnPoint();
            lastShot = (float)game.time;    
        }
        private Vector3 CreateSpawnPoint()
        {   //create random spawn positions for enemies
            int luku = game.random.Next(1,4);//first choose one of each 4 sides of screen
            Vector3 spawnPoint = new Vector3();
            switch(luku){//then based on different sides, calculate random spot in that area just behind the screen border
                case 1://upLeft and downRight are variables calculated from screenspace to worldspace to find out 3d cordinates for screen borders
                    spawnPoint = new Vector3(game.random.Next(-(int)game.downRight.X,(int)game.downRight.X), game.upLeft.Y+5f, 0);
                    break;
                case 2:
                    spawnPoint = new Vector3(game.random.Next(-(int)game.downRight.X, (int)game.downRight.X), -game.upLeft.Y-5f, 0);
                    break;
                case 3:
                    spawnPoint = new Vector3(game.upLeft.X-2, game.random.Next((int)game.downRight.Y, -(int)game.downRight.Y), 0);
                    break;
                case 4:
                    spawnPoint = new Vector3(-game.upLeft.X+2, game.random.Next((int)game.downRight.Y, -(int)game.downRight.Y), 0);
                    break;
            }
            return spawnPoint; 

        }
        public void Update()
        {
            //update new bounding box position to this enemy
            physicsBody = new BoundingBox(position - new Vector3(3, 3, 3), position + new Vector3(3, 3, 3));
            angle = game.LookAt(position,game.player.position,angle,0.08f);
            //if enemy is far away from player, move it towards players position
            if ((position - game.player.position).Length() > 3f)
            {
                flyDir = Vector3.Normalize(game.player.position - position) * enemySpeed;
                position += flyDir;
            }
            //check if enemy is close enough to player, then make it die
            else
            {
                shouldDie = true;
                game.player.health -= 5;
                game.hit.Play(0.8f, 0, 0);
                game.combo = 0;
            }
            if (lastShot + shootDelay < game.time)
            {
                Shoot();
                lastShot = (float)game.time;
            }
        }
        private void Shoot()
        {
            Bullet bullet = new Bullet(position, -Vector3.Normalize(game.player.position - position)*.8f, true, false);
            bullet.Initialize(game, angle);
            if (bullet != null)
                game.bulletArray.Add(bullet);
        }
        //update collisions
        public void UpdateCollision(List<Ufo> boundList, int index)
        {
            bool noCollision = true; //this is checked true always at start
            for (int a = index+2; a < boundList.Count; a++)//loop threw all unchecked physicsbodys
            {
                if (physicsBody.Intersects(boundList[a].physicsBody))//if it touches
                {   //check witch one is closer to player
                    if ((game.player.position - position).Length() < (game.player.position - boundList[a].position).Length())
                    {   //if this one is closer, move it faster
                        enemySpeed += .005f;
                        boundList[a].enemySpeed -= 0.02f;//and other one slower
                        if (boundList[a].enemySpeed < 0.01f)
                            boundList[a].enemySpeed = 0.01f;
                    }
                    else
                    {   //if your slower, tag noCollision false
                        noCollision = false;
                        boundList[a].enemySpeed += .005f;//move other one faster
                        enemySpeed -= 0.02f;//and this one slower
                        if (enemySpeed < 0.01f)
                            enemySpeed = 0.01f;
                    }
                }
            }
            if (noCollision)//if there is no collisions
                if (enemySpeed <= .2f)//increase speed, if its not more than max level
                    enemySpeed += .01f;
        }
        public void Draw()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = texture;
                    effect.World = Matrix.CreateScale(2, 2, 2) * Matrix.CreateRotationZ(angle) * Matrix.CreateTranslation(position);
                    effect.View = game.view;
                    effect.Projection = game.projection;
                }
                mesh.Draw();
            }
        }
    }
}
