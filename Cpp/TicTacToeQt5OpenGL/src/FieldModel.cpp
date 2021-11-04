#include "FieldModel.h"

#include <QImage>
#include <QOpenGLTexture>
#include <QMatrix4x4>
#include <limits>
#include <utility>


namespace constants {
constexpr float size = 2.0f;
constexpr float margin = 0.2f;
constexpr float step = size + margin;

constexpr float height = 1.0f;
constexpr float precision = 0.001f;

constexpr float scale(unsigned max_dim) { return 1.15f / max_dim; }
constexpr float translation(unsigned max_dim) {
  return (size * scale(max_dim) * max_dim / 2)+(margin * scale(max_dim) * (max_dim-1) / 2);
}
} // namespace constants



void FieldModel::create(QOpenGLShaderProgram* program, QMatrix4x4* projection,
                       QMatrix4x4* view, QMatrix4x4* model) {
  m_program = program;

  m_projection = projection;
  m_view = view;
  m_model = model;

  m_mesh = std::make_unique<Mesh>();
  m_textures.emplace_back(std::make_unique<QOpenGLTexture>(QImage(":/textures/box.png").mirrored()));
  m_textures.emplace_back(std::make_unique<QOpenGLTexture>(QImage(":/textures/clear.png").mirrored()));
  m_textures.emplace_back(std::make_unique<QOpenGLTexture>(QImage(":/textures/cross.png").mirrored()));
  m_textures.emplace_back(std::make_unique<QOpenGLTexture>(QImage(":/textures/nought.png").mirrored()));
}


void FieldModel::destroy() {
  m_squares.clear();

  m_textures.clear();

  m_mesh.reset(nullptr);
}

void FieldModel::draw() {
  for (unsigned i=0; i < m_dim_x * m_dim_y; ++i) {
    m_squares[i]->draw();
  }
}


void FieldModel::press(unsigned i) {
  if (m_over || m_squares[i]->getType() != SquareTypes::Clear) return;

  m_squares[i]->setStatus(Statuses::Pressed);
}


void FieldModel::release(unsigned i) {
  if (m_over || m_squares[i]->getType() != SquareTypes::Clear) return;

  m_squares[i]->setStatus(Statuses::Normal);
}


void FieldModel::newGame(unsigned dim_x, unsigned dim_y, unsigned win_size) {
  m_squares.clear();


  m_dim_x = dim_x;
  m_dim_y = dim_y;
  m_win_size = win_size;

  m_over = false;


  m_squares.reserve(m_dim_x * m_dim_y);
  QMatrix4x4 local;
  for (unsigned i=0; i < m_dim_x * m_dim_y; ++i) {
    m_squares.emplace_back(std::make_unique<Square>(m_mesh.get(), &m_textures, m_program, m_model));

    local.setToIdentity();
    local.translate(1.0f, -1.0f, 0.0f);
    local.translate((i % m_dim_x) * constants::step, -1.0f * (i / m_dim_x) * constants::step, 0);


    m_squares[i]->setLocalTransform(local);
  }


  m_model->setToIdentity();
  unsigned max_dim = std::max((m_dim_x), (m_dim_y));
  m_model->translate(-constants::translation(max_dim), constants::translation(max_dim), 0);
  m_model->scale(constants::scale(max_dim));
}


void FieldModel::setSquare(unsigned i, SquareTypes type) {
  m_squares[i]->setType(type);
}


void FieldModel::finishGame(Conditions conditions) {
  unsigned i = conditions.start_i;
  unsigned j = conditions.start_j;

  switch(conditions.direction) {
    case Directions::Right:
      for (unsigned k = 0; k < m_win_size; ++k)
        m_squares[i * m_dim_x + (j + k)]->setStatus(Statuses::WinChain);
      break;
    case Directions::Down:
      for (unsigned k = 0; k < m_win_size; ++k)
        m_squares[(i + k) * m_dim_x + j]->setStatus(Statuses::WinChain);
      break;
    case Directions::RightDown:
      for (unsigned k = 0; k < m_win_size; ++k)
        m_squares[(i + k) * m_dim_x + (j + k)]->setStatus(Statuses::WinChain);
      break;
    case Directions::LeftDown:
      for (unsigned k = 0; k < m_win_size; ++k)
        m_squares[(i + k) * m_dim_x + (j - k)]->setStatus(Statuses::WinChain);
      break;
    default:
      break;
    }

  m_over = true;
}



// ======================RayCasting======================
static std::pair<bool, float> calculateIntersectionWithPlaneRayLength(
    const QVector3D& ray, const QVector3D& camera, const QVector3D& plane_normal, float plane_distance);


// if there is a square under mouse returns index of this square, otherwise returns -1
int FieldModel::getSquareUnderMouse(int screen_x, int screen_y, int width, int height) {
  QVector3D ray = calculateRay(screen_x, screen_y, width, height);

  QVector3D camera(m_model->inverted() * m_view->inverted() * QVector3D());

  return getIntersectedSquareIndex(ray, camera);
}


QVector3D FieldModel::calculateRay(int screen_x, int screen_y, int width, int height) {
  float clip_x = (2.0f * screen_x) / width - 1.0f;
  float clip_y = 1.0f - (2.0f * screen_y) / height;
  QVector4D ray_clip(clip_x, clip_y, -1.0f, 1.0f);


  QVector4D ray_eye(m_projection->inverted() * ray_clip);
  ray_eye.setZ(-1.0f); // directed from camera
  ray_eye.setW(0.0f); // is a direction vector


  QVector3D ray(m_model->inverted() * m_view->inverted() * ray_eye);
  ray.normalize();


  return ray;
}


// walk through every cube and check for intersections with 6 faces of each one
// if intersection point with currently checked cube is found and it is closer to camera then take it
int FieldModel::getIntersectedSquareIndex(const QVector3D& ray, const QVector3D& camera) {
  // each normal corresponds to 2 cube faces
  QVector3D plane_normals[3] = {
    QVector3D(0.0f, 0.0f, 1.0f),  // bottom and top
    QVector3D(0.0f, 1.0f, 0.0f),  // front and back
    QVector3D(1.0f, 0.0f, 0.0f)   // left and right
  };

  // distances to these 2 faces of the cube from point of origin (which is same as in cube mesh creation)
  float plane_distances[3][2] = {
    { 0, 1},  // bottom, top
    {-1, 1},  // front, back
    {-1, 1}   // left, right
  };


  int index = -1;
  float min_length = std::numeric_limits<float>::max();


  for (unsigned k = 0; k < m_squares.size(); ++k) {
    // get cube[k] local origin point relative to world space
    QVector3D origin(m_squares[k]->getLocalTransform() * QVector4D(0.0f, 0.0f, 0.0f, 1.0f));

    for (unsigned i = 0; i < 3; ++i) { // for each of x/y/z directions
      float offset = QVector3D::dotProduct(origin, plane_normals[i]);
      float min_length_inner = std::numeric_limits<float>::max(); // to only do full check of the closest of 2 parallel faces

      for (unsigned j = 0; j < 2; ++j) { // for each 2 faces in this direction
        bool intersected;
        float length;
        std::tie(intersected, length) = calculateIntersectionWithPlaneRayLength(
                         ray, camera, plane_normals[i], offset + plane_distances[i][j]);

        if (intersected && length < min_length_inner)
          min_length_inner = length;
      }

      if (min_length_inner < std::numeric_limits<float>::max() - constants::precision &&
          min_length_inner < min_length) {

        // get intersection coordinates relative to cube[k] local origin point
        QVector3D p(m_squares[k]->getLocalTransform().inverted() * QVector4D(camera + min_length_inner * ray, 1.0f));

        if (p.z() >= 0 - constants::precision && p.z() <= constants::height + constants::precision &&
            -p.y() >= -constants::size/2 - constants::precision && -p.y() <= constants::size/2 + constants::precision &&
            p.x() >= -constants::size/2 - constants::precision && p.x() <= constants::size/2 + constants::precision) {

          min_length = min_length_inner;
          index = static_cast<int>(k);
        }
      }
    }
  }

  return index;
}


// set of points Q on the ray is given by equasion Q = (camera + length * ray)
// set of points P on the plane is given by equasion P * plane_normal = plane_distance
// hence point of their intersection is given by equasion (camera + length * ray) * plane_normal = plane_distance
// therefore length = (plane_distance - camera * plane_normal) / (ray * plane_normal)
// if (ray * plane_normal == 0) ray is parallel to plane => no intersection
// if (length < 0) intersection is behind camera => disregard it
static std::pair<bool, float> calculateIntersectionWithPlaneRayLength(
    const QVector3D& ray, const QVector3D& camera, const QVector3D& plane_normal, float plane_distance) {

  float dot_plane_ray = QVector3D::dotProduct(ray, plane_normal);
  if (std::abs(dot_plane_ray) < constants::precision) return std::pair<bool, float>(false, 0); // ray parallel to plane
  float length = (plane_distance - QVector3D::dotProduct(camera, plane_normal)) / dot_plane_ray;

  if (length < 0) return std::pair<bool, float>(false, 0); // intersection behind camera

  return std::pair<bool, float>(true, length);;
}
// ----------------------RayCasting----------------------
