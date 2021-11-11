using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HengDao.Utils;

namespace HengDao
{
    //public delegate T WittyAction<T, T0>(T0 obj);
    //public class AnimParameters
    //{
    //    public Transform gameObject;
    //    public float duration;
    //    public 
    //}
    public class WittyAnimation
    {
        private static Dictionary<int, Coroutine> coroutines_ = new Dictionary<int, Coroutine>();

        private static int AnimImp(YieldInstructionCreateDelegate instfunc, BoolDelegate boolFunc, System.Action action, System.Action breakFun = null)
        {
            Coroutine coroutine = CoroutineLauncher.current.StartCoroutine(
                Utils.WaitForAndDoLoop(instfunc, boolFunc, action, breakFun));

            int id = coroutine.GetHashCode();
            coroutines_.Add(id, coroutine);
            return id;

        }


        public static void StopAnimation(int anim)
        {
            if(coroutines_.ContainsKey(anim))
            {
                CoroutineLauncher.current.StopCoroutine(coroutines_[anim]);
                coroutines_.Remove(anim);
            }
        }

        public static int TweenPosition(Transform gameObject, Vector3 from, Vector3 to, float speed = 1.0f,System.Action onAnimationEnd = null)
        {
            if (speed < 0.0f)
            {
                speed = 0.0f;
            }

            float t = 0.0f;
            return AnimImp(
            () =>
            {
                return new WaitForEndOfFrame();
            },
           () =>
           {
               return t < 1.0f;
           },
           () =>
           {
               t += Time.deltaTime * speed;
               gameObject.position = Vector3.Lerp(from, to, t);
           },
           onAnimationEnd);
        }

        public static int PingPongPosition(Transform gameObject, Vector3 from, Vector3 to, float speed = 1.0f)
        {
            if (speed < 0.0f)
            {
                speed = 0.0f;
            }

            float t = 0.0f;
            return AnimImp(
            () =>
            {
                return new WaitForEndOfFrame();
            },
           () =>
           {
               return true;
           },
           () =>
           {
               t += Time.deltaTime * speed;
               if(t > 1.0f || t < 0.0f)
               {
                   speed *= -1;
               }
               gameObject.position = Vector3.Lerp(from, to, t);
           });
        }


        public static int TweenPosition(List<Transform> gameObjs,List<Vector3> from,List<Vector3> to,float speed = 1.0f,System.Action onAnimationEnd = null)
        {
            if(speed < 0.0f)
            {
                speed = 0.0f;
            }

            float t = 0.0f;
            return AnimImp(() =>
            {
                return new WaitForEndOfFrame();
            },
           () =>
           {
               return t < 1.0f;
           },
           () =>
           {
               t += Time.deltaTime * speed;
               for(int i=0;i< gameObjs.Count;++i)
               {
                   gameObjs[i].position = Vector3.Lerp(from[i], to[i], t);
               }
           },
           onAnimationEnd);
        }

        public static int TweenRotation(Transform gameObject, Quaternion from, Quaternion to, float speed = 1.0f,System.Action onAnimationEnd = null)
        {
            if (speed < 0.0f)
            {
                speed = 0.0f;
            }

            float t = 0.0f;
            return AnimImp(
            () =>
            {
                return new WaitForEndOfFrame();
            },
           () =>
           {
               return t < 1.0f;
           },
           () =>
           {
               t += Time.deltaTime * speed;
               gameObject.rotation = Quaternion.Lerp(from, to, t);
           },
           onAnimationEnd);

        }

        public static int TweenColor(System.Action<Color> setColor,Color from,Color to,float speed = 1.0f,System.Action onAnimationEnd = null)
        {
            if(speed < 0.0f)
            {
                speed = 0.0f;
            }
            float t = 0.0f;
            return AnimImp(() =>
            {
                return new WaitForEndOfFrame();
            },
            () =>
            {
                return t < 1.0f;
            },
            () =>
            {
                t += Time.deltaTime * speed;
                setColor(Color.Lerp(from, to, t));
            },
            onAnimationEnd);

        }

        public static int TweenFloat(System.Action<float> setFloat, float from, float to, float speed = 1.0f)
        {
            if (speed < 0.0f)
            {
                speed = 0.0f;
            }
            float t = 0.0f;
            return AnimImp(() =>
            {
                return new WaitForEndOfFrame();
            },
            () =>
            {
                return t < 1.0f;
            },
            () =>
            {
                t += Time.deltaTime * speed;
                setFloat(Mathf.Lerp(from, to, t));
            });
        }

        public static int TweenFloatLoop(System.Action<float> setFloat, float from, float to, float speed = 1.0f)
        {
            if (speed < 0.0f)
            {
                speed = 0.0f;
            }
            float t = 0.0f;
            return AnimImp(() =>
            {
                return new WaitForEndOfFrame();
            },
            () =>
            {
                return t < 1.0f;
            },
            () =>
            {
                t += Time.deltaTime * speed;
                if(t >= 1.0f)
                {
                    t = 0.0f;
                }
                setFloat(Mathf.Lerp(from, to, t));
            });
        }

        public void Stop(int coroutine)
        {
            if(coroutines_.ContainsKey(coroutine))
            {
                CoroutineLauncher.current.StopCoroutine(coroutines_[coroutine]);
                coroutines_.Remove(coroutine);
            }
        }
    }
}
