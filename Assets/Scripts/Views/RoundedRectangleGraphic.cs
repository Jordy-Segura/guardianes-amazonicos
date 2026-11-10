using UnityEngine;
using UnityEngine.UI;

namespace GuardianesAmazonicos.Views
{
    [AddComponentMenu("UI/Guardianes/Rounded Rectangle Graphic")]
    public sealed class RoundedRectangleGraphic : MaskableGraphic
    {
        [SerializeField, Min(0f)] private float _radioEsquina = 24f;
        [SerializeField, Range(2, 16)] private int _segmentosPorEsquina = 8;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rectangulo = rectTransform.rect;
            float radio = Mathf.Min(_radioEsquina, Mathf.Min(rectangulo.width, rectangulo.height) * 0.5f);
            Vector2 centro = rectangulo.center;

            var verticeCentro = UIVertex.simpleVert;
            verticeCentro.color = color;
            verticeCentro.position = centro;
            vertexHelper.AddVert(verticeCentro);

            Vector2[] centrosEsquina =
            {
                new Vector2(rectangulo.xMax - radio, rectangulo.yMin + radio),
                new Vector2(rectangulo.xMax - radio, rectangulo.yMax - radio),
                new Vector2(rectangulo.xMin + radio, rectangulo.yMax - radio),
                new Vector2(rectangulo.xMin + radio, rectangulo.yMin + radio)
            };
            float[] angulosInicio = { -90f, 0f, 90f, 180f };
            int indiceAnterior = -1;

            for (int esquina = 0; esquina < centrosEsquina.Length; esquina++)
            {
                for (int segmento = 0; segmento <= _segmentosPorEsquina; segmento++)
                {
                    float angulo = (angulosInicio[esquina] + 90f * segmento / _segmentosPorEsquina) * Mathf.Deg2Rad;
                    Vector2 punto = centrosEsquina[esquina] + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
                    var vertice = UIVertex.simpleVert;
                    vertice.color = color;
                    vertice.position = punto;
                    int indiceActual = vertexHelper.currentVertCount;
                    vertexHelper.AddVert(vertice);

                    if (indiceAnterior >= 0)
                    {
                        vertexHelper.AddTriangle(0, indiceAnterior, indiceActual);
                    }

                    indiceAnterior = indiceActual;
                }
            }

            if (vertexHelper.currentVertCount > 2)
            {
                vertexHelper.AddTriangle(0, indiceAnterior, 1);
            }
        }
    }
}
