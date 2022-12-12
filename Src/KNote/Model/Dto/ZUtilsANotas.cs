using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

[Serializable()]
public class ANotasExport
{
    public List<CarpetaExport> Carpetas { get; set; } = new List<CarpetaExport>();
    public List<EtiquetaExport> Etiquetas { get; set; } = new List<EtiquetaExport>();
}

[Serializable()]
public class CarpetaExport
{
    public int IdCarpeta { get; set; }
    public string NombreCarpeta { get; set; }
    public int IdCarpetaPadre { get; set; }
    public int Orden { get; set; }
    public string OrdenNotas { get; set; }
    public List<CarpetaExport> CarpetasHijas { get; set; } = new List<CarpetaExport>();
    public List<NotaExport> Notas { get; set; }
}

[Serializable()]
public class NotaExport
{
    public int IdNota { get; set; }
    public int IdCarpeta { get; set; }
    public string Asunto { get; set; }
    public string Usuario { get; set; }
    public DateTime FechaHoraCreacion { get; set; }
    public string UsuarioModificacion { get; set; }
    public DateTime FechaModificacion { get; set; }
    public string DescripcionNota { get; set; }
    public string Vinculo { get; set; }
    public string PalabrasClave { get; set; }
    public string NotaEx { get; set; }
    public int Prioridad { get; set; }
    public int NivelDificultad { get; set; }
    public bool Visible { get; set; }
    public bool SiempreArriba { get; set; }
    public bool Resuelto { get; set; }
    public bool Ok { get; set; }
    public double TiempoEstimado { get; set; }
    public double TiempoInvertido { get; set; }
    public DateTime? FechaPrevistaInicio { get; set; }
    public DateTime? FechaPrevistaFin { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public DateTime? Alarma { get; set; }
    public int Estilo { get; set; }
    public int TipoAlarma { get; set; }
    public bool AlarmaOk { get; set; }
    public bool ActivarAlarma { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int Alto { get; set; }
    public int Ancho { get; set; }
    public string FontName { get; set; }
    public int FontSize { get; set; }
    public bool FontStrikethru { get; set; }
    public bool FontUnderline { get; set; }
    public bool FontItalic { get; set; }
    public bool FontBold { get; set; }
    public int ColorNota { get; set; }
    public int ColorTextoBanda { get; set; }
    public int ColorBanda { get; set; }
    public int ForeColor { get; set; }
    public bool OcultarEnInternet { get; set; }
    public bool EstaEnArchivadorPersonal { get; set; }
    public bool SincroAlarmaGoogleCalendar { get; set; }
    public bool FormatoHtml { get; set; }
}

[Serializable()]
public class EtiquetaExport
{
    public int IdEtiqueta { get; set; }
    public string CodPadre { get; set; }
    public string CodEtiqueta { get; set; }
    public string DesEtiqueta { get; set; }
}
