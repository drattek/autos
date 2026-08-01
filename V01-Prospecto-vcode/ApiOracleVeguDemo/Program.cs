using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/oracle/test", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("OracleDb");
    await using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    var result = await conn.QuerySingleAsync<string>("SELECT 'OK' FROM dual");
    return Results.Ok(new { message = result });
});

app.MapGet("/oracle/empresas", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("OracleDb");
    await using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    var empresas = await conn.QueryAsync("SELECT EMPR_EMPRESAID, EMPR_ORGANIZACION, EMPR_NOMBRE, EMPR_RFC, EMPR_MARCA FROM AUTOS.EMPRESAS");
    return Results.Ok(empresas);
});

app.MapGet("/oracle/empresas/{empresaId}", async (int empresaId, IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("OracleDb");
    await using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    var sql = @"SELECT EMPRESAS.EMPR_EMPRESAID,
       EMPRESAS.EMPR_ORGANIZACION,
       EMPRESAS.EMPR_NOMBRE,
       EMPRESAS.EMPR_RFC,
       EMPRESAS.EMPR_MARCA,
       SG_AGENCIA.AGEN_IDAGENCIA,
       SG_AGENCIA.AGEN_NOMAGENCIA
FROM AUTOS.EMPRESAS EMPRESAS
JOIN AUTOS.SG_AGENCIA SG_AGENCIA
  ON EMPRESAS.EMPR_EMPRESAID = SG_AGENCIA.EMPR_EMPRESAID
WHERE EMPRESAS.EMPR_EMPRESAID = :empresaId";

    var result = await conn.QueryAsync(sql, new { empresaId });
    return Results.Ok(result);
});

app.MapPost("/oracle/prospecto", async (ProspectoRequest request, IConfiguration config) =>
{
    var errors = Validate(request);
    if (errors.Count > 0)
    {
        return Results.BadRequest(new { errors });
    }

    var connectionString = config.GetConnectionString("OracleDb");
    await using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    try
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "AUTOS.Osp_Pr_Prospectoventas_Abc";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;

        AddParameter(cmd, "pn_empresaid", OracleDbType.Int32, request.PnEmpresaId);
        var pnProspectoId = AddParameter(cmd, "pn_prospectoid", OracleDbType.Int32, request.PnProspectoId ?? 0, ParameterDirection.InputOutput);
        AddParameter(cmd, "pv_compania", OracleDbType.Varchar2, request.PvCompania);
        AddParameter(cmd, "pv_nombre", OracleDbType.Varchar2, request.PvNombre);
        AddParameter(cmd, "pv_apellidopaterno", OracleDbType.Varchar2, request.PvApellidoPaterno);
        AddParameter(cmd, "pv_apellidomaterno", OracleDbType.Varchar2, request.PvApellidoMaterno);
        AddParameter(cmd, "pv_sexo", OracleDbType.Varchar2, request.PvSexo);
        AddParameter(cmd, "pv_titulo", OracleDbType.Varchar2, request.PvTitulo);
        AddParameter(cmd, "pv_puesto", OracleDbType.Varchar2, request.PvPuesto);
        AddParameter(cmd, "pv_emailpart", OracleDbType.Varchar2, request.PvEmailPart);
        AddParameter(cmd, "pv_telefono1part", OracleDbType.Varchar2, request.PvTelefono1Part);
        AddParameter(cmd, "pv_telefonofaxpart", OracleDbType.Varchar2, request.PvTelefonoFaxPart);
        AddParameter(cmd, "pv_telefonocelpart", OracleDbType.Varchar2, request.PvTelefonoCelPart);
        AddParameter(cmd, "pv_nextelpart", OracleDbType.Varchar2, request.PvNextelPart);
        AddParameter(cmd, "pv_domiciliopart", OracleDbType.Varchar2, request.PvDomicilioPart);
        AddParameter(cmd, "pv_coloniapart", OracleDbType.Varchar2, request.PvColoniaPart);
        AddParameter(cmd, "pv_ciudadpart", OracleDbType.Varchar2, request.PvCiudadPart);
        AddParameter(cmd, "pv_estadopart", OracleDbType.Varchar2, request.PvEstadoPart);
        AddParameter(cmd, "pv_cppart", OracleDbType.Varchar2, request.PvCpPart);
        AddParameter(cmd, "pv_status", OracleDbType.Varchar2, request.PvStatus);
        AddParameter(cmd, "pn_primercontacto", OracleDbType.Int32, request.PnPrimerContacto);
        AddParameter(cmd, "pn_fuenteclave", OracleDbType.Int32, request.PnFuenteClave);
        AddParameter(cmd, "pn_nivelinfluen", OracleDbType.Int32, request.PnNivelInfluen);
        AddParameter(cmd, "pv_fisicamoral", OracleDbType.Varchar2, request.PvFisicaMoral);
        AddParameter(cmd, "pv_estadocivil", OracleDbType.Varchar2, request.PvEstadoCivil);
        AddParameter(cmd, "pv_aficiones", OracleDbType.Varchar2, request.PvAficiones);
        AddParameter(cmd, "pn_hijosmayores", OracleDbType.Int32, request.PnHijosMayores);
        AddParameter(cmd, "pv_emailofic", OracleDbType.Varchar2, request.PvEmailOfic);
        AddParameter(cmd, "pv_telefono1ofic", OracleDbType.Varchar2, request.PvTelefono1Ofic);
        AddParameter(cmd, "pv_telefonofaxofic", OracleDbType.Varchar2, request.PvTelefonoFaxOfic);
        AddParameter(cmd, "pv_telefonocelofic", OracleDbType.Varchar2, request.PvTelefonoCelOfic);
        AddParameter(cmd, "pv_domicilioofic", OracleDbType.Varchar2, request.PvDomicilioOfic);
        AddParameter(cmd, "pv_coloniaofic", OracleDbType.Varchar2, request.PvColoniaOfic);
        AddParameter(cmd, "pv_ciudadofic", OracleDbType.Varchar2, request.PvCiudadOfic);
        AddParameter(cmd, "pv_estadoofic", OracleDbType.Varchar2, request.PvEstadoOfic);
        AddParameter(cmd, "pv_cpofic", OracleDbType.Varchar2, request.PvCpOfic);
        AddParameter(cmd, "pn_vendedorclave", OracleDbType.Int32, request.PnVendedorClave);
        AddParameter(cmd, "pv_tipocompra", OracleDbType.Varchar2, request.PvTipoCompra);
        AddParameter(cmd, "pv_rfc", OracleDbType.Varchar2, request.PvRfc);
        AddParameter(cmd, "pn_avanceclave", OracleDbType.Int32, request.PnAvanceClave);
        AddParameter(cmd, "pv_usuario", OracleDbType.Varchar2, request.PvUsuario);
        AddParameter(cmd, "pn_opcion", OracleDbType.Int32, request.PnOpcion);
        AddParameter(cmd, "pd_fechacumple", OracleDbType.Date, request.PdFechaCumple);
        AddParameter(cmd, "pd_fecha1ercontac", OracleDbType.Date, request.PdFecha1erContac);
        AddParameter(cmd, "pn_cliente", OracleDbType.Int32, request.PnCliente);
        AddParameter(cmd, "pn_lead", OracleDbType.Varchar2, request.PnLead);
        AddParameter(cmd, "pv_CURP", OracleDbType.Varchar2, request.PvCURP);
        AddParameter(cmd, "pv_rfcOFIC", OracleDbType.Varchar2, request.PvRfcOfic);
        AddParameter(cmd, "pv_rangosocial", OracleDbType.Varchar2, request.PvRangoSocial);
        AddParameter(cmd, "pv_escolaridad", OracleDbType.Varchar2, request.PvEscolaridad);
        AddParameter(cmd, "pv_claveedocivil", OracleDbType.Varchar2, request.PvClaveEdoCivil);
        AddParameter(cmd, "pv_identificacion", OracleDbType.Varchar2, request.PvIdentificacion);
        AddParameter(cmd, "pv_nidentificacion", OracleDbType.Varchar2, request.PvNIdentificacion);
        AddParameter(cmd, "pv_cvefuente", OracleDbType.Varchar2, request.PvCveFuente);
        AddParameter(cmd, "pv_CveTipoVta", OracleDbType.Varchar2, request.PvCveTipoVta);
        AddParameter(cmd, "PV_NISSANID", OracleDbType.Varchar2, request.PvNissanId);

        await cmd.ExecuteNonQueryAsync();

        return Results.Ok(new { pn_prospectoid = pnProspectoId.Value });
    }
    catch (OracleException ex)
    {
        return Results.Problem(
            title: "Oracle Error",
            detail: ex.Message,
            statusCode: 500);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Internal Server Error",
            detail: ex.Message,
            statusCode: 500);
    }
});

app.MapGet("/oracle/prospecto/{prospectoId}", async (int prospectoId, IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("OracleDb");
    await using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    var sql = @"SELECT 
VEND_TELEFONO2, VEND_TELEFONO1, VEND_RFC, 
   VEND_IDAGENCIA, VEND_DOMICILIO, VEND_CLAVE, 
   USUARIOMODIFICACION, USUARIOALTA, TELEFONOS, 
   STR_EJECUTIVOVTAS, STR_DATOSPERSONALES, STR_DATOSOFICIONA, 
   REGISTRADO, PROS_VEND_CLAVE, PROS_TITULO, 
   PROS_TIPOCOMPRA, PROS_TELEFONOFAXPART, PROS_TELEFONOFAXOFIC, 
   PROS_TELEFONOCELPART, PROS_TELEFONOCELOFIC, PROS_TELEFONO1PART, 
   PROS_TELEFONO1OFIC, PROS_STATUS, PROS_SEXO, 
   PROS_RFCOFIC, PROS_RFC, PROS_PUESTO, 
   PROS_PROSPECTOID, PROS_PRIM_CLAVE, PROS_NOMBRE, 
   PROS_NIVE_CLAVE, PROS_NEXTELPART, PROS_HIJOSMAYORES, 
   PROS_FUEN_CLAVE, PROS_FISICAMORAL, PROS_FECHACUMPLE, 
   PROS_FECHA1ERCONTAC1, PROS_FECHA1ERCONTAC, PROS_ESTADOPART, 
   PROS_ESTADOOFIC, PROS_ESTADOCIVIL, PROS_EMAILPART, 
   PROS_EMAILOFIC, PROS_DOMICILIOPART, PROS_DOMICILIOOFIC, 
   PROS_CURP, PROS_CPPART, PROS_CPOFIC, 
   PROS_COMPANIA, PROS_COLONIAPART, PROS_COLONIAOFI, 
   PROS_CLIE_CLAVE, PROS_CIUDADPART, PROS_CIUDADOFIC, 
   PROS_AVAN_CLAVE, PROS_APPELLIDOMATERNO, PROS_APELLIDOPATERNO, 
   PROS_AFICIONES, PRIM_DESCRIPCION, PRIM_CLAVE, 
   PERSONA, NOMBREVEND, NOMBREPROS, 
   NIVE_DESCRIPCION, NIVE_CLAVE, MESREGISTRO, 
   FUEN_DESCRIPCION, FUEN_CLAVE, FECHAMODIFICACION, 
   FECHAALTA1, FECHAALTA, EMPR_EMPRESAID, 
   DIAREGISTRO, DIACUMPLEANIOS, DATOSPROSP, 
   AVAN_DESCRIPCION, AGEN_NOMAGENCIA
    FROM AUTOS.PR_VPROSPECTOS
    WHERE PROS_PROSPECTOID = :prospectoId";

    var result = await conn.QueryAsync(sql, new { prospectoId });
    return Results.Ok(result);
});

static List<string> Validate(ProspectoRequest request)
{
    var errors = new List<string>();
    if (request.PnEmpresaId <= 0)
        errors.Add("pn_empresaid es obligatorio y debe ser mayor que 0.");
    if (string.IsNullOrWhiteSpace(request.PvNombre))
        errors.Add("pv_nombre es obligatorio.");
    if (string.IsNullOrWhiteSpace(request.PvApellidoPaterno))
        errors.Add("pv_apellidopaterno es obligatorio.");
    if (string.IsNullOrWhiteSpace(request.PvEmailPart))
        errors.Add("pv_emailpart es obligatorio.");
    if (string.IsNullOrWhiteSpace(request.PvTelefono1Part))
        errors.Add("pv_telefono1part es obligatorio.");
    if (string.IsNullOrWhiteSpace(request.PvRfc))
        errors.Add("pv_rfc es obligatorio.");
    if (string.IsNullOrWhiteSpace(request.PvUsuario))
        errors.Add("pv_usuario es obligatorio.");
    if (request.PnOpcion == null || request.PnOpcion == 0)
        errors.Add("pn_opcion es obligatorio y debe ser diferente de 0.");
    return errors;
}

static OracleParameter AddParameter(OracleCommand cmd, string name, OracleDbType type, object? value, ParameterDirection direction = ParameterDirection.Input)
{
    var parameter = cmd.CreateParameter();
    parameter.ParameterName = name;
    parameter.OracleDbType = type;
    parameter.Direction = direction;
    if (direction == ParameterDirection.Input || direction == ParameterDirection.InputOutput)
    {
        parameter.Value = value ?? DBNull.Value;
    }
    cmd.Parameters.Add(parameter);
    return parameter;
}

// <summary>
// Record that represents the request body for the prospecto endpoint.  
// Internal record ProspectoRequestA( antes estaba aqui, pero lo cambie al Final
// </summary>

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
internal record ProspectoRequest(
    int PnEmpresaId,
    int? PnProspectoId,
    string? PvCompania,
    string? PvNombre,
    string? PvApellidoPaterno,
    string? PvApellidoMaterno,
    string? PvSexo,
    string? PvTitulo,
    string? PvPuesto,
    string? PvEmailPart,
    string? PvTelefono1Part,
    string? PvTelefonoFaxPart,
    string? PvTelefonoCelPart,
    string? PvNextelPart,
    string? PvDomicilioPart,
    string? PvColoniaPart,
    string? PvCiudadPart,
    string? PvEstadoPart,
    string? PvCpPart,
    string? PvStatus,
    int? PnPrimerContacto,
    int? PnFuenteClave,
    int? PnNivelInfluen,
    string? PvFisicaMoral,
    string? PvEstadoCivil,
    string? PvAficiones,
    int? PnHijosMayores,
    string? PvEmailOfic,
    string? PvTelefono1Ofic,
    string? PvTelefonoFaxOfic,
    string? PvTelefonoCelOfic,
    string? PvDomicilioOfic,
    string? PvColoniaOfic,
    string? PvCiudadOfic,
    string? PvEstadoOfic,
    string? PvCpOfic,
    int? PnVendedorClave,
    string? PvTipoCompra,
    string? PvRfc,
    int? PnAvanceClave,
    string? PvUsuario,
    int? PnOpcion,
    DateTime? PdFechaCumple,
    DateTime? PdFecha1erContac,
    int? PnCliente,
    string? PnLead,
    string? PvCURP,
    string? PvRfcOfic,
    string? PvRangoSocial,
    string? PvEscolaridad,
    string? PvClaveEdoCivil,
    string? PvIdentificacion,
    string? PvNIdentificacion,
    string? PvCveFuente,
    string? PvCveTipoVta,
    string? PvNissanId
);
