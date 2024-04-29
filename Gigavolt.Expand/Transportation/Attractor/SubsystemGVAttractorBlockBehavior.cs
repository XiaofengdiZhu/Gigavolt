using System;
using System.Collections.Generic;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVAttractorBlockBehavior : Subsystem, IUpdateable {
        public class MoveParameters {
            public Vector3 Destination;
            public float Speed;
            public bool Rebound;
            public float Damping;
            public DateTime StopTime;
            public float BoxSizeSquaredHalf;
            public bool IsGravityEnabled;
        }

        readonly Dictionary<ComponentBody, MoveParameters> m_updateQueue = new();
        public SubsystemBodies m_subsystemBodies;
        public DateTime m_lastUpdate;

        public UpdateOrder UpdateOrder => UpdateOrder.CreatureModels + 1;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(true);
        }

        public void Update(float dt) {
            DateTime now = DateTime.Now;
            double timeSpan = (now - m_lastUpdate).TotalMilliseconds;
            if (timeSpan > 10) {
                m_lastUpdate = now.AddMilliseconds(-timeSpan % 10);
                if (m_updateQueue.Count == 0) {
                    return;
                }
                Dictionary<ComponentBody, bool> toRemove = new();
                foreach (KeyValuePair<ComponentBody, MoveParameters> pair in m_updateQueue) {
                    ComponentBody body = pair.Key;
                    MoveParameters para = pair.Value;
                    float speedAbs = MathF.Abs(para.Speed);
                    if (speedAbs < 0.3f
                        || now > para.StopTime
                        || Vector3.DistanceSquared(para.Destination, body.Position) <= para.BoxSizeSquaredHalf) {
                        toRemove.Add(body, para.IsGravityEnabled);
                        continue;
                    }
                    if (body.IsGravityEnabled) {
                        body.IsGravityEnabled = false;
                    }
                    Vector3 destinationDirection = (para.Destination - body.Position) * (para.Rebound ? -1f : 1f);
                    float distance = destinationDirection.Length();
                    float speedTowardDestination = Vector3.Dot(body.Velocity, destinationDirection) / distance;
                    if (speedTowardDestination < speedAbs) {
                        body.ApplyImpulse(Vector3.LimitLength(destinationDirection, speedAbs - speedTowardDestination));
                    }
                    if (para.Rebound) {
                        if (para.Rebound) {
                            para.Speed += para.Damping;
                        }
                        else {
                            para.Speed -= para.Damping;
                        }
                    }
                }
                foreach (KeyValuePair<ComponentBody, bool> pair in toRemove) {
                    if (pair.Key.IsGravityEnabled != pair.Value) {
                        pair.Key.IsGravityEnabled = pair.Value;
                    }
                    m_updateQueue.Remove(pair.Key);
                }
            }
        }

        public override void OnEntityRemoved(Entity entity) {
            foreach (ComponentBody componentBody in entity.FindComponents<ComponentBody>()) {
                m_updateQueue.Remove(componentBody);
            }
        }

        public void StartMoveBody(ComponentBody body, Vector3 destination, float speed, bool rebound) {
            if (m_updateQueue.TryGetValue(body, out MoveParameters parameter)) {
                parameter.Destination = destination;
                parameter.Speed = speed;
                parameter.Rebound = rebound;
                parameter.StopTime = DateTime.Now.AddMinutes(1);
            }
            else {
                m_updateQueue.Add(
                    body,
                    new MoveParameters {
                        Destination = destination,
                        Speed = speed,
                        Rebound = rebound,
                        Damping = MathF.Log2(body.Mass) / 200f,
                        StopTime = DateTime.Now.AddMinutes(1),
                        BoxSizeSquaredHalf = body.BoxSize.LengthSquared() / 4,
                        IsGravityEnabled = body.IsGravityEnabled
                    }
                );
            }
        }
    }
}