#include "inset.h"
#include "../geometry_ops.h"

DEFAULT_CGAL_TYPES()
DEFAULT_ARRANGEMENT_TYPES()
CGAL_EXACT_TYPES2()
CGAL_EXACT_ARRANGEMENT()

struct ssk_observer : straight_skeleton_observer
{
	ssk_observer(double amount, geometry::vertex2_list& vl, exact_vertex2_list& svl) :
		amount_(amount),
		vl_(vl),
		svl_(svl)
	{
	}

	virtual bool get_bisector(ss_segment& s1, ss_segment& s2, exact_line2& result) { return false; }
	virtual bool collision_line(int idx1, int idx2, exact_line2& result) { return false; }
	virtual void boundary(exact_edge2 r, edge3 he, double& t, int& idx) {}
	virtual void boundary(straight_skeleton_observer::edge2 r, straight_skeleton_observer::edge he, double& t, int& idx)
	{
		t = amount_;
		idx = 0;

		vl_.push_back(he->target());
		svl_.push_back(r->target());
	}

	double amount_;
	geometry::vertex2_list&	vl_;
	exact_vertex2_list& svl_;
};

struct tessk_observer : arrangement_in_face2<ExactKernel, exact_arrangement2, DoubleKernel, geometry::arrangement2>::observer
{
	tessk_observer(straight_skeleton& ss, inset_labels& labels, face2_list* result, face2_list* sides) :
		_ss(ss),
		_labels(labels),
		_result(result),
		_sides(sides)
	{
	}

	virtual void vertex_created(vertex2 v, exact_vertex2 v2) {}
	virtual void edge_created(edge2 he, exact_edge2 he2) {}

	virtual void face_created(face2 f, exact_face2 f2)
	{
		int idx = _ss.foreign_id(f2);
		if (idx < 0)
		{
			_labels.apply_result_face(f->data().label);

			if (_labels.modifies_result_edges())
			{
				edge2_circulator eit = f->outer_ccb();
				edge2_circulator end = eit;
				CGAL_For_all(eit, end)
				{
					_labels.apply_result_edge(eit);
				}
			}

			if (_result)
				_result->push_back(f);
		}
		else
		{
			//side face
			_labels.apply_side_face(f);
			if (_sides)
				_sides->push_back(f);
		}
	}

private:
	straight_skeleton& _ss;
	inset_labels& _labels;
	face2_list* _result;
	face2_list* _sides;
};

//inset
void inset::run(double amount, inset_request& request, inset_labels& labels)
{
	if (amount <= 0.0)
	{
		//td: error
		throw "inset must be a positive number";
	}

	//build the inset in 2d, keep track of boundaries
	vertex2_list       vl;
	exact_vertex2_list svl;

	straight_skeleton ss;
	ssk_observer      sso(amount, vl, svl);

	ss.build(request.f, &sso);

	tessk_observer to(ss, labels, request.faces, request.sides);

	arrangement_in_face2<ExactKernel, exact_arrangement2, DoubleKernel, arrangement2> tess(ss.result(), request.arr, request.f);
	tess.add_observer(&to);
	tess.build(vl, svl);
}
