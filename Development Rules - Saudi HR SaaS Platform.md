# Development Rules — Saudi HR SaaS Platform

## Purpose

This document defines the mandatory development standards for all future features, modules, pages, services, workflows, reports, dashboards, and integrations within the HR SaaS Platform.

The platform must be built as a configurable HR Operating System rather than a collection of isolated HR modules.

# Core Rule

Every future feature must be designed and implemented as:

Frontend + Backend

Never design frontend only.

Never design backend only.

Every feature request must include both layers.

# Required Response Structure

Every future module, enhancement, or feature must be designed using the following structure:

## 1\. Business Overview

Explain:

- Business purpose
- User problem solved
- Target users
- Impact on HR operations

## 2\. Frontend Architecture

Always define:

### Pages

Routes  
Navigation  
Sidebar placement

### Components

Tables  
Cards  
Forms  
Drawers  
Modals  
Tabs  
Kanban Boards  
Calendars  
Graphs  
Widgets

### UI Behavior

User actions  
Empty states  
Loading states  
Error states  
Success states  
Permission visibility

### UX Requirements

The UI must follow:

HubSpot  
Linear  
Stripe  
Notion  
ClickUp

Avoid:

Old HR systems  
Table-only interfaces  
Static pages  
Complex navigation

### Frontend Stack

Next.js  
TypeScript  
Tailwind CSS  
shadcn/ui  
Framer Motion  
RTL Support

## 3\. Backend Architecture

Always define:

### Domain Entities

### DTOs

### CQRS Commands

### CQRS Queries

### Services

### Validation Rules

### Business Logic

### Authorization Rules

### Background Jobs

### Event Handling

### Workflow Integration

### Automation Integration

### Audit Logging

Backend Stack:

ASP.NET Core  
.NET 8  
PostgreSQL  
Entity Framework Core  
Dapper  
Redis  
Cloudflare R2

## 4\. Database Design

Always define:

### New Tables

### Updated Tables

### Relationships

### Constraints

### Indexes

### Multi-Tenant Fields

Every table must support:

TenantId  
CreatedAt  
CreatedBy  
UpdatedAt  
UpdatedBy  
DeletedAt  
DeletedBy  
IsDeleted

## 5\. API Design

Always provide:

### Endpoints

### Request DTOs

### Response DTOs

### Pagination

### Filtering

### Sorting

### Permission Requirements

### Error Handling

## 6\. Workflow Integration

Always explain:

What workflows are triggered  
What approvals are required  
What dynamic approvers are used  
What automations are executed

## 7\. Permissions

Always define:

### Roles

### Permission Templates

### User Overrides

### Scope Rules

Supported scopes:

Company  
Branch  
Department  
Direct Reports  
Own Data  
Custom Groups

## 8\. Notifications

Always define:

### Notification Events

### Notification Channels

Supported channels:

In-App  
Email  
Push Notifications  
WhatsApp Future Support

## 9\. Reports & Dashboards Impact

Always explain:

### Dashboard Widgets

### Reports

### Analytics Objects

### KPIs

### Export Requirements

Supported exports:

PDF  
XLSX

## 10\. Audit Logs

Always define:

### What is logged

### Old Value

### New Value

### User

### Timestamp

### Source

### Related Entity

Every sensitive action must be auditable.

## 11\. Development Tasks

Always break implementation into:

### Backend Tasks

### Frontend Tasks

### Database Tasks

### Integration Tasks

### Testing Tasks

# Platform Architecture Rule

Every future module must consume the platform engines.

Do NOT build isolated modules.

All modules must integrate with the platform ecosystem.

# Platform Engines

Every feature must leverage:

Metadata Engine  
Object Registry Engine  
Permission Engine  
Workflow Engine  
Automation Engine  
Audit Engine  
Timeline Engine  
Document Token Engine  
Dashboard Engine  
Notification Engine

# Metadata-Driven Principle

Avoid hardcoded logic whenever possible.

Examples:

Leave Types  
Request Types  
Allowance Types  
Deduction Types  
Document Types  
Workflow Types  
Dashboard Templates  
Report Templates  
Attendance Policies  
Shift Types

All should be configurable from system settings.

# Object-Based Architecture

All system data should be treated as reusable objects.

Examples:

Employee  
Department  
Attendance Record  
Leave Request  
Payroll Item  
Expense Claim  
Loan  
Document  
Task  
Workflow Instance

Objects power:

Reports  
Dashboards  
Widgets  
Analytics  
Exports  
AI Features

# Multi-Tenant Rule

Every query, service, workflow, report, dashboard, and API must respect:

Tenant Isolation

No tenant can access another tenant’s data.

# Frontend + Backend Rule

Every future prompt must include:

Frontend Design  
Backend Design  
Database Design  
API Design  
Workflow Design  
Permissions  
Notifications  
Reports  
Audit Logs

No exceptions.

# Pre-Implementation Checklist

Before implementing any feature ask:

How does it affect Employees?  
<br/>How does it affect Workflows?  
<br/>How does it affect Permissions?  
<br/>How does it affect Reports?  
<br/>How does it affect Dashboards?  
<br/>How does it affect Audit Logs?  
<br/>How does it affect Notifications?  
<br/>How does it affect Automations?

If any of these are affected, they must be included in the implementation design.

# Final Rule

The platform is not a traditional HR system.

The platform is a:

Configurable HR Operating System

Every feature must be:

Reusable  
Configurable  
Metadata-Driven  
Workflow-Aware  
Permission-Aware  
Tenant-Aware  
Reportable  
Auditable  
Dashboard-Compatible

Build platform engines first.

Build business modules on top of those engines.