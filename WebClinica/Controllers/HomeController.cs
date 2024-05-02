using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebClinica.Models;
using WebClinica.Models.Entidades;

namespace WebClinica.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View();
            }
        }

        public ActionResult ValidarUsuarioContrasenia(Usuarios u)
        {
            try
            {
                Usuarios usu = new Usuarios();
                List<Pacientes> ls = new List<Pacientes>();

                using (Generacion24Entities db = new Generacion24Entities())
                {
                    usu = db.Usuarios.Where(x => x.NickName == u.NickName && x.Contrasenia == u.Contrasenia).FirstOrDefault();
                }

                if (usu == null)
                {
                    throw new Exception("Usuario y/o Contraseña incorrecto");
                }

                //Se estan guardando los datos del usuario en sesion
                Session["usuario"] = usu;

                return RedirectToAction("Pacientes");
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View("Login");
            }
        }

        public ActionResult Pacientes()
        {
            ClienteconCantactos obj = new ClienteconCantactos();

            try
            {
                if (Session["usuario"] == null)
                {
                    return View("Login");
                }

                //se obtienen los datos de sesion
                Usuarios usu = (Usuarios)Session["usuario"];

                List<Pacientes> ls = new List<Pacientes>();
                List<PacientesContacto> lsPc = new List<PacientesContacto>();

                //aqui recuperamos los contactos del usuario
                using (Generacion24Entities db = new Generacion24Entities())
                {
                    //muestra los pacientes del usuario logeado
                    ls = db.Pacientes.Where(x => x.UsuarioId == usu.Id).ToList();
                    lsPc = db.PacientesContacto.Include("MediosContacto").Include("Pacientes").ToList();
                }

                obj.listaPacientes = ls;
                obj.listaPacientesContacto = lsPc;

                return View(obj);
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;

                return View(new List<Pacientes>());
            }
        }

        public ActionResult CerrarSession()
        {
            try
            {
                Session["usuario"] = null;
                return View("Login");
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View("Login");
            }
        }

        public ActionResult Create()
        {
            try
            {
                if (Session["usuario"] == null)
                {
                    return View("Login");
                }

                List<MediosContacto> ls = new List<MediosContacto>();
                using (Generacion24Entities db = new Generacion24Entities())
                {
                    ls = db.MediosContacto.ToList();
                }

                ViewBag.MedioContactoId = new SelectList(ls, "Id", "Tipo");

                return View();
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View();
            }
        }

        public ActionResult CreatePost(Pacientes p, int MedioContactoId, string info,HttpPostedFileBase ArchivoImagen)
        {
            try
            {
                if (Session["usuario"] == null)
                {
                    return View("Login");
                }

                string rutaArchivo = Path.Combine(Server.MapPath("~/Images"), p.Nombre + p.Id+ArchivoImagen.FileName);

                ArchivoImagen.SaveAs(rutaArchivo);

                Usuarios usu = (Usuarios)Session["usuario"]; ;
                p.UsuarioId = usu.Id;

                PacientesContacto pc = new PacientesContacto();
                pc.MedioContactoId = MedioContactoId;
                pc.Informacion = info;

                using (Generacion24Entities db = new Generacion24Entities())
                {
                    p.Imagen = p.Nombre + p.Id + ArchivoImagen.FileName;

                    db.Pacientes.Add(p);
                    db.SaveChanges();

                    pc.PacienteId = p.Id;

                    db.PacientesContacto.Add(pc);
                    db.SaveChanges();
                }

                TempData["msj"] = "se agrego el paciente " + p.Nombre;


                return RedirectToAction("Pacientes");
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View();
            }
        }

        public ActionResult Registrar()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View();
            }
        }

        public ActionResult RegistrarPost(Usuarios u)
        {
            try
            {
                using (Generacion24Entities db = new Generacion24Entities())
                {
                    db.Usuarios.Add(u);
                    db.SaveChanges();
                }
                TempData["msj"] = $"Se registro correctamente al usuario {u.NickName} ya puedes iniciar sesion";
                return View("Login");
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View("Registrar");
            }

        }

        public ActionResult Contactos(int id)//id del paciente
        {
            List<PacientesContacto> lsPc = new List<PacientesContacto>();
            List<MediosContacto> lsMc = new List<MediosContacto>();

            try
            {
                using(Generacion24Entities db = new Generacion24Entities())
                {
                    lsPc = db.PacientesContacto.Include("MediosContacto").Include("Pacientes")
                            .Where(x => x.PacienteId == id).ToList();

                    lsMc = db.MediosContacto.ToList();
                }

                ViewBag.MedioContactoId = new SelectList(lsMc,"Id","Tipo");
                TempData["idPaciente"] = id;

                return View(lsPc);
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return RedirectToAction("Pacientes");
            }
        }


        public ActionResult DeleteContacto(int id)//id del contacto
        {
            PacientesContacto pc = new PacientesContacto();
            int idPaciente = 0;

            try
            {
                using (Generacion24Entities db = new Generacion24Entities())
                {
                    pc = db.PacientesContacto.Where(x=> x.Id == id).FirstOrDefault();
                    idPaciente = pc.PacienteId.Value;

                    db.PacientesContacto.Remove(pc);
                    db.SaveChanges();
                }

                TempData["msj"] = "Se borro correctamente";

                return RedirectToAction("Contactos", new {id = idPaciente});
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return RedirectToAction("Pacientes");
            }
        }


        public ActionResult Agregarcontacto(PacientesContacto pc)
        {
            try
            {
                using (Generacion24Entities db = new Generacion24Entities())
                {
                    db.PacientesContacto.Add(pc);
                    db.SaveChanges();
                }

                    return RedirectToAction("Contactos",new {id = pc.PacienteId});
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return View();
            }
        }

        public ActionResult DeletePaciente(int id)
        {
            try
            {
                using(Generacion24Entities db = new Generacion24Entities())
                {
                    Pacientes p = new Pacientes();

                    p = db.Pacientes.Where(x=> x.Id == id).FirstOrDefault();
                    db.Pacientes.Remove(p);
                    db.SaveChanges();

                }

                return RedirectToAction("Pacientes");
            }
            catch (Exception ex)
            {
                TempData["msj"] = ex.Message;
                return RedirectToAction("Pacientes");
            }
        }




    }
}