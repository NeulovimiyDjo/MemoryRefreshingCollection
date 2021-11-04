#ifndef FIELD_MODEL_H
#define FIELD_MODEL_H

#include "Square.h"
#include "Mesh.h"

#include <vector>
#include <memory>

class QOpenGLShaderProgram;
class QOpenGLTexture;
class QMatrix4x4;


class FieldModel {
public:
  void create(QOpenGLShaderProgram* program, QMatrix4x4* projection,
              QMatrix4x4* view, QMatrix4x4* model);
  void destroy();

  void newGame(unsigned dim_x, unsigned dim_y, unsigned win_size);
  void setSquare(unsigned i, SquareTypes type);
  void finishGame(Conditions conditions);

  void draw();

  int getSquareUnderMouse(int x, int y, int w, int h);
  void press(unsigned i);
  void release(unsigned i);

private:
  QVector3D calculateRay(int x, int y, int w, int h);
  int getIntersectedSquareIndex(const QVector3D& ray, const QVector3D& camera);


  std::unique_ptr<Mesh> m_mesh;
  std::vector<std::unique_ptr<QOpenGLTexture>> m_textures;

  std::vector<std::unique_ptr<Square>> m_squares;
  unsigned m_dim_x;
  unsigned m_dim_y;
  unsigned m_win_size;
  bool m_over;

  QOpenGLShaderProgram* m_program = nullptr;

  QMatrix4x4* m_projection = nullptr;
  QMatrix4x4* m_view = nullptr;
  QMatrix4x4* m_model = nullptr;
};

#endif // FIELD_MODEL_H
