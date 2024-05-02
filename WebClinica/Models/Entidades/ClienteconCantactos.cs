using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebClinica.Models.Entidades
{
    public class ClienteconCantactos
    {

        public List<Pacientes> listaPacientes { get; set; }

        public List<PacientesContacto> listaPacientesContacto { get; set; }

    }
}