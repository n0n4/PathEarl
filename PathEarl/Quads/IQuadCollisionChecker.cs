using System;
using System.Collections.Generic;
using System.Text;

namespace PathEarlCore.Quads
{
    public interface IQuadCollisionChecker<T>
    {
        bool Collides(Quadentry<T> a, Quadentry<T> b);
    }
}
