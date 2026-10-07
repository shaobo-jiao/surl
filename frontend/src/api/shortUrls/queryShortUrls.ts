import axios from "axios";
import apiClient from "../apiClient";
import { GeneralFailureResult, type ProblemDetails, type Result } from "../shared";
import { toShortUrl, type ShortUrl, type ShortUrlResponse } from "./shortUrl.shared";

// properties the backend allows ordering the query result by
export type QueryShortUrlsOrderBy = "expiresAt" | "clickCount" | null;

export interface QueryShortUrlsParams {
  expiresFrom?: Date | null,
  expiresTo?: Date | null,
  orderBy?: QueryShortUrlsOrderBy,
  isDescending?: boolean,
  page?: number,
  pageSize?: number,
}

// validation fields returned by the backend for invalid query params
export type QueryShortUrlsFields =
  "expiresFrom" | "expiresTo" | "orderBy" | "isDescending" | "page" | "pageSize";

export interface PagedResult<T> {
  items: T[],
  page: number,
  pageSize: number,
  totalCount: number,
  totalPages: number,
}

/**
 * Queries the authenticated user's short URLs.
 * Omitted params fall back to the backend defaults (page 1, page size 10).
 */
export async function queryShortUrlsApi(queryParams: QueryShortUrlsParams = {})
  : Promise<Result<PagedResult<ShortUrl>, ProblemDetails<QueryShortUrlsFields>>> {
  try {
    const { data } = await apiClient.get<PagedResult<ShortUrlResponse>>("short-urls/", {
      params: {
        expiresFrom: queryParams.expiresFrom?.toISOString(),
        expiresTo: queryParams.expiresTo?.toISOString(),
        orderBy: queryParams.orderBy,
        isDescending: queryParams.isDescending,
        page: queryParams.page,
        pageSize: queryParams.pageSize,
      }
    });
    
    return {
      success: true,
      data: { ...data, items: data.items.map(toShortUrl) }
    }
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response)
      return GeneralFailureResult;
    return { success: false, error: error.response.data };
  }
}
